using Newtonsoft.Json.Linq;
using Octokit;
using PDBot.Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.API
{
    partial class BuggedCards
    {
        private static GitHubClient _githubClient;

        private static GitHubClient GithubClient
        {
            get
            {
                if (_githubClient == null)
                {
                    _githubClient = new GitHubClient(new ProductHeaderValue("PennyDreadfulMtg-PDBot"));
                    var path = "github_token.txt";
                    if (!File.Exists(path)) {
                        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "github_token.txt");
                    }

                    if (!File.Exists(path))
                    {
                        System.Diagnostics.Process.Start("notepad.exe", path).WaitForExit(10000);
                    }

                    if (File.Exists(path))
                    {
                        var tokenAuth = new Credentials(File.ReadAllText(path));
                        _githubClient.Credentials = tokenAuth;
                        Console.WriteLine($"Authorized to Github with `{tokenAuth}`");
                    }
                    else
                    {
                        Console.WriteLine("Not Authorized to Github!!!");
                    }

                }
                return _githubClient;
            }
        }

        private static Task<Repository> GetRepositoryAsync()
        {
            return GithubClient.Repository.Get("PennyDreadfulMTG", "modo-bugs");
        }

        private static async Task<Project> GetVerificationProjct()
        {
            var repo = await GetRepositoryAsync();
            return (await GithubClient.Repository.Project.GetAllForRepository(repo.Id)).Single();
        }

        public static async Task<(bool success, string message)> UpdateBuggedAsync(string CardName, string Player, int MatchID, bool isFixed)
        {
            if (string.IsNullOrEmpty(CardName))
            {
                return (false, "You need to specify the card name.");
            }
            try
            {
                CheckForNewList();
                var bug = Bugs.SingleOrDefault(n => n.CardName == CardName);
                if (bug != null)
                {
                    var repo = await GetRepositoryAsync();
                    var num = int.Parse(bug.Url.Split('/').Last());
                    var issue = await GithubClient.Issue.Get(repo.Id, num);

                    var verification = await GetVerificationForBugAsync(issue.Number);
                    if (verification != null && verification.Equals(await GetCurrentBuildAsync()))
                    {
                        return (true, "Thanks, but our information about this bug is already up to date.");
                    }

                    var md_link = $"in match [{MatchID}](https://logs.pennydreadfulmagic.com/match/{MatchID}/)";
                    if (MatchID == -1)
                    {
                        md_link = "on discord";
                    }

                    string stillBuggedText;
                    if (isFixed)
                    {
                        stillBuggedText = $"Fixed according to `{Player}` {md_link}.";
                    }
                    else
                    {
                        stillBuggedText = $"Still bugged according to `{Player}` {md_link}.";
                    }

                    await GithubClient.Issue.Comment.Create(repo.Id, issue.Number, stillBuggedText);

                    if (!isFixed)
                    {
                        var currentCol = await GetLatestColumnAsync();
                        if (verification == null)
                        {
                            await GithubClient.Repository.Project.Card.Create(currentCol.Id, new NewProjectCard(issue.Id, ProjectCardContentType.Issue));
                        }
                        else
                        {
                            var card = await GetCardForBugAsync(issue.Number);
                            await GithubClient.Repository.Project.Card.Move(card.Id, new ProjectCardMove(ProjectCardPosition.Top, currentCol.Id, null));
                        }
                        Verifications[issue.Number] = await GetCurrentBuildAsync();
                        return (true, "Thanks, I've updated my records!");
                    }
                    else
                    {
                        return (true, "Thanks, Our team will review this match shortly");
                    }

                }
                return (false, "I couldn't find a bug for that card.");
            }
            catch (Exception c)
            {
                var msg = $"Error updating Modo-bugs:\nCardName={CardName}\n{c}";
                await DiscordService.SendToTestAsync(msg);
                Console.WriteLine(msg);
            }
            return (false, "Sorry, I encountered an error.  Please PM me the details.");
        }

        private static async Task<ProjectColumn> GetLatestColumnAsync()
        {
            var build = await GetCurrentBuildAsync();
            var proj = await GetVerificationProjct();
            var columns = await GithubClient.Repository.Project.Column.GetAll(proj.Id);
            var latest = columns.FirstOrDefault(c => c.Name == build.ToString());
            if (latest == null)
            {
                latest = await GithubClient.Repository.Project.Column.Create(proj.Id, new NewProjectColumn(build.ToString()));
            }

            return latest;
        }

        private static readonly Dictionary<int, Version> Verifications = new Dictionary<int, Version>();

        public static async Task<Version> GetCurrentBuildAsync()
        {
            using (var wc = new WebClient())
            {
                var blob = await wc.DownloadStringTaskAsync(new Uri("https://pennydreadfulmtg.github.io/modo-bugs/mtgo_version.json"));
                var json = JObject.Parse(blob);
                return Version.Parse(json.Value<string>("version"));
            }
        }

        public static async Task<Version> GetVerificationForBugAsync(int IssueNumber)
        {
            if (Verifications.ContainsKey(IssueNumber))
                return Verifications[IssueNumber];
            var proj = await GetVerificationProjct();
            var columns = await GithubClient.Repository.Project.Column.GetAll(proj.Id);
            foreach (var col in columns)
            {
                if (col.Name == "Needs Testing")
                {
                    continue;
                }

                foreach (var card in await GithubClient.Repository.Project.Card.GetAll(col.Id)) {
                    var num = int.Parse(card.ContentUrl.Split('/').Last());
                    Verifications[num] = Version.Parse(col.Name);
                }
                if (Verifications.ContainsKey(IssueNumber))
                    return Verifications[IssueNumber];
            }

            return null;
        }

        public static async Task<ProjectCard> GetCardForBugAsync(int IssueNumber)
        {
            var proj = await GetVerificationProjct();
            var columns = await GithubClient.Repository.Project.Column.GetAll(proj.Id);
            foreach (var col in columns)
            {
                foreach (var card in await GithubClient.Repository.Project.Card.GetAll(col.Id))
                {
                    var num = int.Parse(card.ContentUrl.Split('/').Last());
                    if (col.Name != "Needs Testing")
                        Verifications[num] = Version.Parse(col.Name);
                    if (num == IssueNumber)
                        return card;
                }
            }

            return null;
        }
    }
}
