using NUnit.Framework;
using NUnit.Framework.Legacy;
using PDBot.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tests.Mocks;

namespace Tests
{
    [Parallelizable(ParallelScope.Children)]
    class FunctionalTest
    {
        private const string LogDirectory = "Logs";

        public static IEnumerable<TestCaseData> Logs()
        {
            var folder = new FileInfo(typeof(FunctionalTest).Assembly.Location).Directory;
            folder = folder.Parent.Parent.Parent.EnumerateDirectories(LogDirectory).FirstOrDefault();
            foreach (var logfile in folder.EnumerateFiles("*.txt"))
            {
                var lines = File.ReadAllLines(logfile.FullName);
                var format = (MagicFormat)Enum.Parse(typeof(MagicFormat), lines[0]);
                var comment = lines[1];
                var observers = lines[2].Split(',');
                var players = lines[3].Split(',');

                lines = lines.SkipWhile(l => !string.IsNullOrWhiteSpace(l)).ToArray(); // Skip all the headers.
                var match = new MockMatch(comment, players, format);

                yield return new TestCaseData(match, observers, lines).SetName($"TestLog({logfile.Name})");

            }
        }

        [Test, TestCaseSource(nameof(Logs))]
        public void TestLog(MockMatch match, string[] expectedObservers, string[] lines)
        {
            var foundObservers = match.Observers.Select(o => o.GetType().Name).ToArray();
            foreach (var observer in expectedObservers)
            {
                ClassicAssert.Contains(observer.Trim(), foundObservers);
            }

            var GameHeader = new Regex(@"== Game (?<n>[0-9]) \((?<gameId>[0-9]+)\) ==");

            var gameID = 0;
            var gameNum = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var input = lines[i];
                Match rxmatch;
                if (input.StartsWith("[CHAT]"))
                    continue;
                else if ((rxmatch = GameHeader.Match(input)).Success)
                {
                    int.TryParse(rxmatch.Groups["gameId"].Value, out gameID);
                    ClassicAssert.AreEqual(++gameNum, int.Parse(rxmatch.Groups["n"].Value), "Game Number was not as expected");
                }
                else if (input.StartsWith("Winner:"))
                {
                    var winner = input.Split(':')[1].TrimStart();
                    match.Winners.Add(gameID, winner);
                    // This is where we'd invoke observer[].ProcessWinner
                    // But I don't trust the LeagueObserver not to submit results.
                }
                else if (input.StartsWith("Match Winner:"))
                {
                    var parts = input.Split(':');
                    match.Winners.GetRecordData(out var winner, out var record);
                    ClassicAssert.AreEqual(winner.Player, parts[1].Trim());
                    ClassicAssert.AreEqual(record, parts[2].Trim());
                }
                else
                {
                    var gameLogLine = new PDBot.Core.Data.GameLogLine(input, match);
                    foreach (var item in match.Observers)
                    {
                        var output = item.HandleLine(gameLogLine);
                    }
                }
            }
        }
    }
}
