using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FB2Library;

namespace Murtlap
{
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