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

namespace discordMusicBot.src.Web
{
    class rule34
    {


        public string[] rule34QuerrySite(string tag)
        {
            try
            {
                string ParseValue = $"http://rule34.paheal.net/api/danbooru/find_posts/index.xml?tags={tag}";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ParseValue);
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
                        //urlResponce = urlResponce.Remove(0, 38);

                        var dic = XDocument
                            .Parse(urlResponce)
                            .Descendants("Post")
                            .ToDictionary(
                                c => c.Attribute("file_url").Value,
                                c => Convert.ChangeType(
                                    c.Value, 
                                    GetType(c.Attribute("DataType").Value)
                                )
                            );

                        string[] returned = null;
                        return returned;
                    }
                }
            }
            catch(Exception error)
            {
                return null;
            }


        }

        private static Type GetType(string type)
        {
            switch (type)
            {
                case "Integer":
                    return typeof(int);
                case "String":
                    return typeof(string);
                case "Boolean":
                    return typeof(bool);
                // TODO: add any other types that you want to support
                default:
                    throw new NotSupportedException(
                        string.Format("The type {0} is not supported", type)
                    );
            }
        }

    }
}
