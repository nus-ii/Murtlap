using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Murtlap
{
    class TranslatorCover
    {
        public static void Translate(CommonData commonData)
        {
            commonData.yTranslator = new YTranslator();
            List<WordItem> items= commonData.wordItemsHandler.wordItems;

            foreach(var i in items)
            {
                i.translate=YTranslator.Translate(i.value);

                foreach(var w in i.contexts)
                {
                    w.Value.translate= YTranslator.Translate(w.Value.value);
                }

                string output = JsonConvert.SerializeObject(new { data = items });
                File.WriteAllText(@"C:\temp\withContextwithTranslate.json", output);
            }

            Console.ReadLine();
        }
    }
}