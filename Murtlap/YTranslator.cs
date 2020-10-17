using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Murtlap
{
    class YTranslator
    {
        string folderId = "";

        string iamToken = "";

        public static string Translate(string value)
        {
            try
            {
                var requestBody = new RecToYat(value);
                string output = JsonConvert.SerializeObject(requestBody);

                HttpClient client = new HttpClient();
                //client.BaseAddress=new Uri(@"https://translate.api.cloud.yandex.net/translate/v2/translate");

                client.DefaultRequestHeaders.Add("Authorization", "");
                var content = new StringContent(output, Encoding.UTF8, "application/json");
                var result = client.PostAsync(@"https://translate.api.cloud.yandex.net/translate/v2/translate", content).Result;

                var res = result.Content.ReadAsStringAsync().Result;

                var deserializedProduct = JsonConvert.DeserializeObject<ResFromYat>(res);
                
                return deserializedProduct.translations.First().text;
            }
            catch (Exception)
            {

                return "";
            }
        }


    }


    class ResFromYat
    {
        public List<ResItem> translations;
    }

    class ResItem
    {
        public string text;
    }

    class RecToYat
    {
        public string sourceLanguageCode = "it";

        public string targetLanguageCode = "ru";

        public string format = "PLAIN_TEXT";

        public List<string> texts;

        public string folderId = "";


        public RecToYat(string val)
        {
            texts = new List<string>();
            texts.Add(val);
        }
    }
}
