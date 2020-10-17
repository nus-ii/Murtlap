using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Murtlap
{
    class UniReader
    {
        public static async void MakeFrequencyDictionary(CommonData commonData)
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

        internal static void FindGoodContextAuto(CommonData obj)
        {
            List<WordItem> items = obj.wordItemsHandler.wordItems;
            foreach(var i in items)
            {
                i.GoodContext=i.contexts.Where(c => c.Value.translate.ToLower().Split(' ').Any(s=>s==i.translate.ToLower())).Select(c=>c.Key).ToList();
            }

            string output = JsonConvert.SerializeObject(new { data = items });
            File.WriteAllText(@"C:\temp\withGoodContext.json", output);


        }

        internal static void ReadCsv(CommonData obj)
        {
            obj.wordItemsHandler = new WordItemsHandler(@"C:\temp\1outMM7583cd39.csv");
            obj.wordItemsHandler.PrintStat();
        }

        internal static void ReadJson(CommonData obj)
        {
            Console.WriteLine("Input path");
            string path = Console.ReadLine();

            obj.wordItemsHandler = new WordItemsHandler(path, FileType.Json);

            obj.wordItemsHandler.PrintStat();
        }

        internal static async void UpdateContext(CommonData commonData)
        {
            var mostPopular = commonData.wordItemsHandler.GetFirst(700);
            Dictionary<int, string> data = FbReaderCover.ReadDataFromFile(@"C:\temp\1.fb2").Result.Select((element, index) => new { element, index }).ToDictionary(ele => ele.index, ele => ele.element);


            foreach (var item in mostPopular)
            {
                item.contexts = GetContext(data, item.value).Select((element, index) => new { element, index }).ToDictionary(ele => ele.index, ele => new Word(ele.element));
            }



            string output = JsonConvert.SerializeObject(new { data = mostPopular });
            File.WriteAllText(@"C:\temp\withContext.json", output);
        }

        private static List<string> GetContext(Dictionary<int, string> data, string item)
        {
            //'!','?','.',',',';','-','—','…','«','»' { '.', '!', '?', '…', '«', '-', '—' }; '»',
            List<string> contextCandidate = new List<string>();
            List<char> startContextMarker = new List<char> { '.', '!', '?', '…', '—', ':', ';' };
            List<char> endContextMarker = new List<char> { '.', '!', '?', '…', '—', ':', ';' };
            //var lowItem = item.ToLower();

            foreach (var line in data)
            {
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
                    contextCandidate.Add(temp.Trim().Trim(',').Trim('«').Trim('»'));
                }
            }


            List<string> result = new List<string>();

            if (contextCandidate.Count > 0)
            {
                if (contextCandidate.Count !=0&&contextCandidate.Count<5)
                    result= contextCandidate.OrderBy(c => c.Length).ToList();


                if (contextCandidate.Count >= 5)
                {
                    result = contextCandidate.OrderBy(c => c.Length).Skip(4).Where(i=>i.Length<200).Take(5).ToList();
                }
            }

            return result;
        }
    }
}