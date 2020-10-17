using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Murtlap
{
    class WordItemsHandler
    {
        public List<WordItem> wordItems { get; set; }

        private List<string> stat;

        public WordItemsHandler()
        {
            wordItems = new List<WordItem>();
            stat = new List<string>();
        }

        public WordItemsHandler(string path, FileType type = FileType.Csv) : this()
        {
            if (type == FileType.Csv)
            {
                var fileData = File.ReadLines(path);
                foreach (var line in fileData)
                {
                    var linesParts = line.Split(';');
                    wordItems.Add(new WordItem()
                    {
                        value = linesParts[0],
                        count = Int32.Parse(linesParts[1])
                    });

                }
            }

            if (type == FileType.Json)
            {
                var fileData = File.ReadAllText(path);
                var tttt = JObject.Parse(fileData);
                JArray hj = (JArray)tttt["data"];

                foreach (var i in hj)
                {
                    wordItems.Add(JsonConvert.DeserializeObject<WordItem>(i.ToString()));
                }
            }


            UpdateStat();
        }


        public List<string> GetCsvData()
        {
            List<string> result = new List<string>();
            var ordered = wordItems.OrderByDescending(i => i.count);
            result = ordered.Select(i => $"{i.value};{i.count}").ToList();
            return result;
        }

        public void AddWordFromBookString(string bookString)
        {
            var candidates = bookString.Split(' ').Select(c => MakeCleanWord(c)).Where(c => !string.IsNullOrEmpty(c)).Where(c => c.Length >= 2);

            var newCandidates = candidates.Where(c => !wordItems.Select(i => i.value).Contains(c));
            var seenCandidates = candidates.Where(c => !newCandidates.Contains(c));
            wordItems.AddRange(newCandidates.Select(c => new WordItem { value = c, count = 1 }));


            foreach (var sc in seenCandidates)
            {
                var v = wordItems.Where(i => sc == i.value).First();
                v.count = v.count + 1;
            }

        }

        private string MakeCleanWord(string s)
        {
            //c.Trim().Trim(',').Trim('.').Trim('-').Trim('—').Replace(';',' ').Trim(':').Trim('…')

            List<char> badChars = new List<char>
            {
                '!','?','.',',',';','-','—','…','«','»'
            };

            foreach (var c in badChars)
            {
                if (s.Contains(c))
                { s = s.Replace(c.ToString(), ""); }
            }

            //if (badChars.Any(i => s.Contains(i)))
            //{
            //    return "";
            //}
            return s;
        }

        public void UpdateStat()
        {

            var topCount = wordItems.Select(i => i.count).Distinct().OrderByDescending(o => o).Take(35);
            var min = topCount.Count() > 0 ? topCount.Min() : 0;
            var ordered = wordItems.Where(i => i.count > min);
            ordered = ordered.Where(i => topCount.Contains(i.count)).OrderByDescending(i => i.count).Take(35);

            stat = new List<string>();
            stat.Add("wordItems: " + wordItems.Count().ToString());
            stat.Add("__________________________________________");
            Console.WriteLine(wordItems.Count());
            stat.AddRange(ordered.Select(o => $"{o.value} {o.count}"));
        }

        public void PrintStat()
        {
            foreach (var i in stat)
            {

                Console.WriteLine(i);
            }
        }

        internal IEnumerable<WordItem> GetFirst(int v)
        {
            return wordItems.Take(v);
        }
    }
}