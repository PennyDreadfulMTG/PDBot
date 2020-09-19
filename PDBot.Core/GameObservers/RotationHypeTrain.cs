using PDBot.Core.API;
using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    class RotationHypeTrain : IGameObserver
    {
        public RotationHypeTrain()
        {
        }

        public bool PreventReboot => false;

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (ShouldJoin(match))
            {
                Task.Factory.StartNew(async () =>
                 {

                     await Task.Delay(TimeSpan.FromSeconds(2));
                     var rotation = await DecksiteApi.GetRotationAsync();
                     match.SendChat($"Penny Dreadful rotates in {rotation.FriendlyDiff}!  Get hyped!");
                 }).GetAwaiter();
                return Task.FromResult<IGameObserver>(this);
            }

            return Task.FromResult<IGameObserver>(null);
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            return null;
        }

        public void ProcessWinner(string winner, int gameID)
        {
            return;
        }

        public bool ShouldJoin(IMatch match)
        {
            return false;
            if (match.Format == MagicFormat.PennyDreadful)
            {
                var rotation = DecksiteApi.GetCachedRotationAsync().GetAwaiter().GetResult();
                var diff = TimeSpan.FromSeconds(rotation.Diff);
                if (diff.TotalHours < 24)
                {
                    if (File.Exists("suppress_hype.txt"))
                        return false;
                    return true;
                }
                if (File.Exists("suppress_hype.txt"))
                {
                    try
                    {
                        File.Delete("suppress_hype.txt");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("[Error] Could not delete suppress_hype.txt?");
                    }
                }
            }
            return false;
        }
    }
}
