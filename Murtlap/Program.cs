using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FB2Library;
using System.IO;
using System.Linq;
using MenuMasterLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Murtlap
{
    class Program
    {
        static void Main(string[] args)
        {
            var Menu = new MenuMasterAction<CommonData>();
            Menu.AddItem("Make frequency dictionary", UniReader.Make);
            Menu.AddItem("Open frequency dictionary from file (.csv)", UniReader.ReadCsv);
            Menu.AddItem("Open frequency dictionary from file (.json)", UniReader.ReadJson);
            Menu.AddItem("Update context", UniReader.UpdateContext);
            Menu.AddItem("Translate", TranslatorCover.Translate);
            Menu.AddItem("Set part of speech flag", TranslatorCover.Translate);
            Menu.PrintAndWait(new CommonData());

            Console.ReadLine();

        }


    }

    class TranslatorCover
    {
        public static void Translate(CommonData commonData)
        {
            commonData.yTranslator = new YTranslator();
            List<WordItem> items= commonData.wordItemsHandler.wordItems;

            foreach(var i in items)
            {
                i.valueTranslate=YTranslator.Translate(i.value);
                i.contextTranslate = YTranslator.Translate(i.context);
            }

            YTranslator.Translate(items.First().value);

            string output = JsonConvert.SerializeObject(new { data = items });
            File.WriteAllText(@"C:\temp\withContextwithTranslate.json", output);





            Console.ReadLine();
        }
    }






    class CommonData
    {
        public WordItemsHandler wordItemsHandler;
        public YTranslator yTranslator;
    }

    class UniReader
    {
        public static async void Make(CommonData commonData)
        {

            List<string> data = await FbReaderCover.ReadDataFromFile(@"C:\temp\1.fb2");
            Double fCount = 0;
            commonData.wordItemsHandler = new WordItemsHandler();
            foreach (string line in data)
            {
                fCount++;
                commonData.wordItemsHandler.AddWordFromBookString(line);

                if (fCount < 20 || fCount % 200 == 0)
                    commonData.wordItemsHandler.UpdateStat();

                if (fCount < 20 || fCount % 20 == 0)
                {
                    Console.Clear();
                    //Console.WriteLine("lines: " + fCount);//4150
                    Console.WriteLine(Math.Round((fCount / 4150) * 100, 2) + "%");//4150
                    commonData.wordItemsHandler.PrintStat();
                }
            }

            string rnd = Guid.NewGuid().ToString().Split('-')[0];
            File.WriteAllLines(@"C:\temp\1outMM" + rnd + ".csv", commonData.wordItemsHandler.GetCsvData().ToArray());
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>");
        }

        internal static void ReadCsv(CommonData obj)
        {
            obj.wordItemsHandler = new WordItemsHandler(@"C:\temp\1outMM7583cd39.csv");
            obj.wordItemsHandler.PrintStat();
        }

        internal static void ReadJson(CommonData obj)
        {
            obj.wordItemsHandler = new WordItemsHandler(@"C:\temp\withContext.json", FileType.Json);
            obj.wordItemsHandler.PrintStat();
        }

        internal static async void UpdateContext(CommonData commonData)
        {
            var mostPopular = commonData.wordItemsHandler.GetFirst(600);//.OrderBy(i => i.count).Take(50);
            Dictionary<int, string> data = FbReaderCover.ReadDataFromFile(@"C:\temp\1.fb2").Result.Select((element, index) => new { element, index }).ToDictionary(ele => ele.index, ele => ele.element);


            foreach (var item in mostPopular)
            {
                item.context = GetContext(data, item.value);
            }



            string output = JsonConvert.SerializeObject(new { data = mostPopular });
            File.WriteAllText(@"C:\temp\withContext.json", output);

            var t = 0;

        }

        private static string GetContext(Dictionary<int, string> data, string item)
        {
            //'!','?','.',',',';','-','—','…','«','»' { '.', '!', '?', '…', '«', '-', '—' }; '»',
            List<string> contextCandidate = new List<string>();
            List<char> startContextMarker = new List<char> { '.', '!', '?', '…', '—', ':', ';' };
            List<char> endContextMarker = new List<char> { '.', '!', '?', '…', '—', ':', ';' };
            //var lowItem = item.ToLower();

            foreach (var line in data)
            {
                if (line.Value.Contains("Somiglio forse al giovane esaltato vagabondo che sarà giustiziato oggi"))
                {
                    var yyyyy = 0;
                }
                //var splited = line.Value.Split(' ');
                item = " " + item.Trim() + " ";
                if (line.Value.Contains(item))
                {
                    var index = line.Value.IndexOf(item);
                    var startContextCandidate = line.Value.Substring(0, index);
                    var endContextCandidate = line.Value.Substring(index + item.Length, line.Value.Length - index - item.Length);
                    string temp = item;

                    for (int i = startContextCandidate.Length - 1; i >= 0; i--)
                    {
                        char c = startContextCandidate[i];
                        if (!startContextMarker.Contains(c))
                        {
                            temp = c.ToString() + temp;
                        }
                        else
                        {
                            if (c == '—')
                            { temp = c.ToString() + temp; }
                            break;
                        }
                    }

                    // temp = temp + " " + item + " ";

                    for (int i = 0; i < endContextCandidate.Length; i++)
                    {
                        char c = endContextCandidate[i];

                        if (!endContextMarker.Contains(c))
                        {
                            temp = temp + c.ToString();

                        }
                        else
                        {
                            if (c != '—')
                            {
                                temp = temp + c.ToString();
                            }
                            break;
                        }
                    }

                    if (temp.Contains("di Kiriat."))
                    {
                        var yyyyy = 0;
                    }

                    contextCandidate.Add(temp.Trim().Trim(',').Trim('«').Trim('»'));
                }
            }


            string result = "";

            if (contextCandidate.Count > 0)
            {
                if (contextCandidate.Count !=0)
                    result= contextCandidate.OrderBy(c => c.Length).First();


                if (contextCandidate.Count >= 5)
                {
                    var tempResult= contextCandidate.OrderBy(c => c.Length).Skip(4).First();

                    if(tempResult.Length<200)
                    {
                        result=tempResult;
                    }
                    
                }
                    

            }

            return result;
        }
    }

    class FbReaderCover
    {
        public static async Task<List<string>> ReadDataFromFile(string path)
        {
            List<string> data = new List<string>();
            FB2Reader reader = new FB2Reader();
            FB2File file = await reader.ReadAsync(File.OpenRead(path), new XmlLoadSettings(new System.Xml.XmlReaderSettings()));

            foreach (var i in file.MainBody.Sections)
            {
                foreach (var ic in i.Content)
                {
                    data.Add(ic.ToString());
                }
            }
            return data;
        }
    }

    enum FileType
    {
        Csv,
        Json
    }


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

    class WordItem
    {
        public string value;

        public int count;

        public string context;

        public string valueTranslate;

        public string contextTranslate;
    }
}
