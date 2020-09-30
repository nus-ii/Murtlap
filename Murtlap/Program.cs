using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FB2Library;
using System.IO;
using System.Linq;
using MenuMasterLib;


namespace Murtlap
{
    class Program
    {
        static void Main(string[] args)
        {
            var Menu = new MenuMasterAction<CommonData>();
            Menu.AddItem("Make frequency dictionary", UniReader.Read);
            Menu.AddItem("Translate", Translator.Translate);
            Menu.PrintAndWait(new CommonData());

            Console.ReadLine();

        }


    }

    class Translator
    {
        public static void Translate(CommonData commonData)
        {
            Console.ReadLine();
        }
    }



    class CommonData
    {

    }

    class UniReader
    {
        public static async void Read(CommonData commonData)
        {

            List<string> data = await FbReaderCover.ReadDataFromFile(@"C:\temp\1.fb2");
            Double fCount = 0;
            WordItemsHendler wih = new WordItemsHendler();
            foreach (string line in data)
            {
                fCount++;
                wih.AddWordFromBookString(line);

                if (fCount < 20 || fCount % 200 == 0)
                    wih.UpdateStat();

                if (fCount < 20 || fCount % 20 == 0)
                {
                    Console.Clear();
                    //Console.WriteLine("lines: " + fCount);//4150
                    Console.WriteLine(Math.Round((fCount / 4150) * 100, 2) + "%");//4150
                    wih.PrintStat();
                }
            }

            string rnd = Guid.NewGuid().ToString().Split('-')[0];
            File.WriteAllLines(@"C:\temp\1outMM"+ rnd + ".csv", wih.GetCsvData().ToArray());
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>");
        }
    }

    class FbReaderCover
    {
        public static async Task<List<string>> ReadDataFromFile(string path)
        {
            List<string> data = new List<string>();
            var r = new FB2Reader();
           // File.OpenRead();
            var ttt = await r.ReadAsync(File.OpenRead(path), new XmlLoadSettings(new System.Xml.XmlReaderSettings()));  

            foreach (var i in ttt.MainBody.Sections)
            {
                foreach (var ic in i.Content)
                {
                    data.Add(ic.ToString());
                }
            }
            return data;
        }
    }

    class WordItemsHendler
    {
        private List<WordItem> wordItems;

        private List<string> stat;

        public WordItemsHendler()
        {
            wordItems = new List<WordItem>();
            stat = new List<string>();
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
            var candidates = bookString.Split(' ').Select(c => c.Trim().Trim(',').Trim('.').Trim('-')).Where(c=>!string.IsNullOrEmpty(c)).Where(c=>c.Length>=4);

            var newCandidates = candidates.Where(c => !wordItems.Select(i => i.value).Contains(c));
            var seenCandidates = candidates.Where(c => !newCandidates.Contains(c));
            wordItems.AddRange(newCandidates.Select(c => new WordItem { value = c, count = 1 }));

            //var seenWordItems = wordItems.Where(i => seenCandidates.Contains(i.value));
            foreach (var sc in seenCandidates)
            {
                var v = wordItems.Where(i => sc==i.value).First();
                v.count = v.count + 1;
            }
                                                  
        }

        public void UpdateStat()
        {

            var topCount = wordItems.Select(i => i.count).Distinct().OrderByDescending(o=>o).Take(35);
            var min =topCount.Count()>0? topCount.Min():0;         
            var ordered = wordItems.Where(i => i.count > min);
            ordered = ordered.Where(i => topCount.Contains(i.count)).OrderByDescending(i => i.count).Take(35);

            stat = new List<string>();
            stat.Add("wordItems: "+wordItems.Count().ToString());
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
    }

    class WordItem
    {
        public string value;

        public int count;
    }
}
