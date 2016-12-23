using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;

namespace discordMusicBot.src.Web
{

    public class listUrbanDictionary
    {
        public string[] tags { get; set; }
        public string result_type { get; set; }
        public List<listUrbanDictionaryDefine> list { get; set; }
        public List<string> sounds { get; set; }
    }

    public class listUrbanDictionaryDefine
    {
        public string definition { get; set; }
        public string permalink { get; set; }
        public int thumbs_up { get; set; }
        public string author { get; set; }
        public string word { get; set; }
        public int defid { get; set; }
        public string current_vote { get; set; }
        public string example { get; set; }
        public int thumbs_down { get; set; }
    }

    public class listUrbanDictionaryTags
    {
        public string definition { get; set; }
        public string tag { get; set; }
    }

    class urban
    {

        logs _logs = new logs();

        public static List<listUrbanDictionaryTags> urbanTags = new List<listUrbanDictionaryTags>();

        public void saveUrbanTags()
        {
            try
            {
                string loc = Directory.GetCurrentDirectory() + "\\configs\\urbanTags.json";
                string json = JsonConvert.SerializeObject(urbanTags);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "urban.saveUrbanTags", error.ToString(), "system");
            }
        }

        public void loadUrbanTags()
        {
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\configs\\urbanTags.json"))
                {
                    string json = File.ReadAllText(Directory.GetCurrentDirectory() + "\\configs\\urbanTags.json");

                    urbanTags = JsonConvert.DeserializeObject<List<listUrbanDictionaryTags>>(json);
                }
                else
                {
                    saveUrbanTags();
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "urban.loadUrbanTags", error.ToString(), "system");
            }
        }

        public string[] cmd_urbanFlow(string term)
        {
            try
            {

                string[] returnedDefinition = null;
                if(term == null)
                {
                    // we are going to pick a random term
                    string returnedTerm = pickRandomDefinition();

                    returnedDefinition = cmd_pickTermDefinition(returnedTerm);

                    return returnedDefinition;
                }
                else
                {
                    returnedDefinition = cmd_pickTermDefinition(term);

                    return returnedDefinition;
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "urban.loadUrbanTags", error.ToString(), "system");
                return null;
            }
        }

        public string[] cmd_pickTermDefinition(string term)
        {
            try
            {
                string ParseValue = $"http://api.urbandictionary.com/v0/define?term={term}";
                //string ParseValue = $"http://www.urbandictionary.com/random.php";

                List<listUrbanDictionary> tempList = new List<listUrbanDictionary>();
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

                        // We got the stream recieved!
                        // Let's ReadToEnd (meaning, let's read to the end of the stream, it outputs the whole stream obtained as a string)
                        var json = readStream.ReadToEnd();

                        var t = JsonConvert.DeserializeObject<listUrbanDictionary>(json);

                        if(t.list.Count == 0)
                        {
                            string[] noResult = { "No Value" };
                            return noResult;
                        }
                        else
                        {
                            //going to pick a random definition for the tag that was given
                            Random rng = new Random();
                            int counter = rng.Next(0, t.list.Count);

                            string tags = null;
                            for (int i = 0; i < t.list.Count; i++)
                            {
                                tags += $"{t.tags[i]}, ";
                            }

                            string[] result = { t.list[counter].definition, t.list[counter].example, term, tags };

                            urbanUpdateTags(t.tags, term);

                            return result;
                        }


                    }
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "urban.cmd_urbanDic", error.ToString(), "system");
                return null;
            }

        }

        private void urbanUpdateTags(string[] tags, string term)
        {
            try
            {
                //load the file into memory 
                if(urbanTags.Count == 0)
                {
                    loadUrbanTags();
                }

                List<string> t = new List<string>();

                t.AddRange(tags);

                //check all the values to see if they exist in the list already
                for(int i = 0; i < t.Count; i++)
                {
                    //this is needed for the first entry
                    if(urbanTags != null)
                    {

                        var urlResult = urbanTags.FindIndex(x => x.tag == t[i]);
                        if (urlResult == -1)
                        {
                            //not found in the file, add it
                            urbanTags.Add(new listUrbanDictionaryTags
                            {
                                definition = term,
                                tag = t[i]
                            });
                        }                        
                    }
                    else
                    {
                        //int c = urbanTags.Count;
                        urbanTags.Add(new listUrbanDictionaryTags
                        {
                            definition = term,
                            tag = t[i]
                        });
                    }
                }

                //all the new items where added to the list now save
                saveUrbanTags();
                
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "urban.cmd_urbanDic", error.ToString(), "system");
            }
            
        }

        private string pickRandomDefinition()
        {
            try
            {
                Random rng = new Random();
                int counter = 0;

                //check to see if anything is in urbanTags
                if (urbanTags.Count == 0)
                {
                    loadUrbanTags();

                    //if we STILL have 0 throw out 'cheesy ragu'
                    if (urbanTags.Count == 0)
                    {
                        cmd_pickTermDefinition("cheesy ragu");
                    }

                    //get a random term from the urbanTags and pass it
                    counter = rng.Next(0, urbanTags.Count);

                    return urbanTags[counter].tag;

                }
                else
                {
                    //get a random term from the urbanTags and pass it
                    counter = rng.Next(0, urbanTags.Count);
                    return urbanTags[counter].tag;
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "urban.cmd_urbanDic", error.ToString(), "system");
                return null;
            }
        }
    }
}
