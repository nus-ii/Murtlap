using System;
using System.Collections.Generic;
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
            var menu = new MenuMasterAction<CommonData>();
            menu.AddItem("Make frequency dictionary", UniReader.MakeFrequencyDictionary);
            menu.AddItem("Open frequency dictionary from file (.csv)", UniReader.ReadCsv);
            menu.AddItem("Open frequency dictionary from file (.json)", UniReader.ReadJson);
            menu.AddItem("Update context", UniReader.UpdateContext);
            menu.AddItem("Translate", TranslatorCover.Translate);
            menu.AddItem("Auto find good context", UniReader.FindGoodContextAuto);
            menu.AddItem("Set part of speech flag", TranslatorCover.Translate);
            
            menu.PrintAndWait(new CommonData());

            Console.ReadLine();

        }


    }


    class CommonData
    {
        public WordItemsHandler wordItemsHandler;
        public YTranslator yTranslator;
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
}
