using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using Newtonsoft.Json;

namespace discordMusicBot.src.Web
{
    class discordStatus
    {

        public class ListDiscordStatusPage
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string updated_at { get; set; }
        }

        public class ListDiscordStatusComponent
        {
            public string status { get; set; }
            public string name { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public int position { get; set; }
            public string description { get; set; }
            public string group_id { get; set; }
            public string id { get; set; }
            public string page_id { get; set; }
            public bool group { get; set; }
            public bool only_show_if_degraded { get; set; }
        }

        public class ListDiscordStatusAffectedComponent
        {
            public string name { get; set; }
        }

        public class ListDiscordStatusIncidentUpdate
        {
            public string status { get; set; }
            public string body { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string display_at { get; set; }
            public List<ListDiscordStatusAffectedComponent> affected_components { get; set; }
            public string id { get; set; }
            public string incident_id { get; set; }
        }

        public class ListDiscordStatusIncident
        {
            public string name { get; set; }
            public string status { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public object monitoring_at { get; set; }
            public object resolved_at { get; set; }
            public string shortlink { get; set; }
            public string id { get; set; }
            public string page_id { get; set; }
            public List<ListDiscordStatusIncidentUpdate> incident_updates { get; set; }
            public string impact { get; set; }
        }

        public class ListDiscordStatusStatus
        {
            public string indicator { get; set; }
            public string description { get; set; }
        }

        public class ListDiscordStatusRootObject
        {
            public ListDiscordStatusPage page { get; set; }
            public List<ListDiscordStatusComponent> components { get; set; }
            public List<ListDiscordStatusIncident> incidents { get; set; }
            public List<object> scheduled_maintenances { get; set; }
            public ListDiscordStatusStatus status { get; set; }
        }

        logs _logs = new logs();

        public string[] getCurrentStatus()
        {
            try
            {

                string rawJSON = requestJsonData();
                string[] parsedJSON = parseJsonData(rawJSON);

                return parsedJSON;
                
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "discordStatus.getCurrentStatus", error.ToString(), "system");
                return null;
            }
        }

        private string requestJsonData()
        {
            try
            {
                string url = "https://srhpyqt94yxb.statuspage.io/api/v2/summary.json";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

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
                        string json = readStream.ReadToEnd();

                        return json;
                    }
                }
            }
            catch
            {

                return null;
            }
        }

        private string[] parseJsonData(string rawJson)
        {
            try
            {
                var t = JsonConvert.DeserializeObject<ListDiscordStatusRootObject>(rawJson);

                //check for any incidents and highlight whats going on
                if (t.incidents.Count >= 1)
                {
                    for (int i = 0; i < t.incidents.Count; i++)
                    {

                    }
                }

                //find values
                int apiInt = t.components.FindIndex(x => x.name == "API");
                int gatewayInt = t.components.FindIndex(x => x.name == "Gateway");
                int cloudFlairInt = t.components.FindIndex(x => x.name == "CloudFlare");
                int voiceInt = t.components.FindIndex(x => x.name == "Voice");

                //clean up the text
                removeSpcialCharacters(t.components[apiInt].status);

                string apiStatus = string.Format("{0,-10} {1,20}", "API:", $"{t.components[apiInt].status}");
                string gatewayStatus = string.Format("{0,-10} {1,14}", $"Gateway:", $"{t.components[gatewayInt].status}");
                string cloudFlairStatus = string.Format("{0,-10} {1,13}", $"CloudFlare:", $"{t.components[cloudFlairInt].status}");
                string voiceStatus = string.Format("{0,-10} {1,18}", $"Voice:", $"{t.components[voiceInt].status}");

                string[] returnResult = { apiStatus, gatewayStatus, cloudFlairStatus, voiceStatus };
                return returnResult;
            }
            catch(Exception error)
            {

                return null;
            }

        }

        private string removeSpcialCharacters(string value)
        {
            try
            {
                if(value == "operational")
                {
                    value.Remove(0, 1);
                    value = "O" + value;
                    return value;
                }

                return null;
            }
            catch(Exception error)
            {

                return null;
            }
        }
    }
}
