using System;
using System.Text;
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
}
