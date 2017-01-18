using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using discordMusicBot.src.sys;

namespace discordMusicBot.src.Web
{
    class rule34
    {
        logs _logs = new logs();

        public class ListRule34
        {
            public int id { get; set; }
            public string md5 { get; set; }
            public string file_name { get; set; }
            public string file_url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
            public string preview_url { get; set; }
            public int preview_height { get; set; }
            public int preview_width { get; set; }
            public string rating { get; set; }
            public string date { get; set; }
            public string is_warehoused { get; set; }
            public string tags { get; set; }
            public string source { get; set; }
            public int score { get; set; }
            public string author { get; set; }
        }

        public async Task<string[]> rule34QuerrySite(string tag)
        {
            try
            {
                await Task.Delay(1);

                string url = null;
                
                if (tag != null)
                {
                    url = $"https://rule34.xxx//index.php?page=dapi&s=post&q=index&tags={tag}";
                }
                else
                {
                    url = "https://rule34.xxx//index.php?page=dapi&s=post&q=index";
                }

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = WebRequestMethods.Http.Get;

                // This goes out and actually performs the web client request sorta thing	
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    // Get the stream associated with the response.
                    // so the webpage ran, it did it's thing, now we need the stream of data obtained
                    Stream receiveStream = response.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {

                        string urlResponce = readStream.ReadToEnd();

                        List<ListRule34> xmls =
                            (from xml in XDocument.Parse(urlResponce).Root.Elements("post")
                             select new ListRule34
                             {
                                 id = (int)xml.Attribute("id"),
                                 md5 = (string)xml.Attribute("md5"),
                                 file_name = (string)xml.Attribute("file_name"),
                                 file_url = (string)xml.Attribute("file_url"),
                                 height = (int)xml.Attribute("height"),
                                 width = (int)xml.Attribute("width"),
                                 preview_url = (string)xml.Attribute("preview_url"),
                                 preview_height = (int)xml.Attribute("preview_height"),
                                 preview_width = (int)xml.Attribute("preview_width"),
                                 rating = (string)xml.Attribute("rating"),
                                 date = (string)xml.Attribute("date"),
                                 is_warehoused = (string)xml.Attribute("is_warehoused"),
                                 tags = (string)xml.Attribute("tags"),
                                 source = (string)xml.Attribute("source"),
                                 score = (int)xml.Attribute("score"),
                                 author = (string)xml.Attribute("author")
                             }).ToList();

                        if(xmls.Count >= 1)
                        {
                            Random rng = new Random();

                            int counter = rng.Next(0, xmls.Count);

                            bool loop = true;
                            while (loop == true)
                            {
                                if (xmls[counter].file_url != "")
                                {
                                    if(tag != null)
                                    {
                                        string[] returnResult = { "https:" + xmls[counter].file_url, "rule34", xmls[counter].tags, tag };
                                        return returnResult;
                                    }
                                    else
                                    {
                                        string[] returnResult = { "https:" + xmls[counter].file_url, "rule34", xmls[counter].tags, "*" };
                                        return returnResult;
                                    }

                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                        return null;
                    }
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "rule34.rule34QuerrySite", error.ToString(), "system");
                return null;
            }


        }

    }
}
