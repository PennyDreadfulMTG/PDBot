using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PDBot.API
{
    public static class BuggedCards
    {
        public class Bug
        {
            public string CardName { get; set; }
            public string Classification { get; set; }
            public string Description { get; set; }
            public string LastConfirmed { get; set; }

            public override string ToString()
            {
                return $"{CardName} - {Description}";
            }
        }

        public static List<Bug> Bugs = new List<Bug>();

        static DateTime LastUpdate;

        public static void CheckForNewList()
        {
            try
            {

                if (DateTime.Now.Subtract(LastUpdate).TotalHours > 1)
                {
                    Bugs.Clear();
                    WebClient wc = new WebClient();
                    var blob = wc.DownloadString("https://pennydreadfulmtg.github.io/modo-bugs/bugs.tsv").Split('\n');
                    Bugs.AddRange(from line in blob
                                  let col = line.Split('\t')
                                  where col.Length > 1
                                  select new Bug()
                                  {
                                      CardName = col[0],
                                      Description = col[1],
                                      Classification = col[2],
                                      //LastConfirmed = col[3]
                                  }
                    );
                    LastUpdate = DateTime.Now;
                }
            }
            catch (WebException c)
            {
                Console.WriteLine($"Failed to update bugged cards\n{c}");
            }
        }

        public static Bug IsCardBugged(string CardName)
        {
            CheckForNewList();
            return Bugs.FirstOrDefault(n => n.CardName == CardName);
        }
    }
}
