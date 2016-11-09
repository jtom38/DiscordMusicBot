using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discordMusicBot.src
{
    public class configuration
    {
        /// <summary> Your bot's command prefix. Please don't pick `!`. </summary>
        public char Prefix { get; set; }
        /// <summary> Ids of users who will have owner access to the bot. </summary>
        public ulong[] Owners { get; set; }
        /// <summary> Your bot's login token. </summary>
        public string Token { get; set; }
        /// <summary> The ID of the room the bot should bind to on startup </summary>
        public ulong[] BindToChannels { get; set; }

        public configuration()
        {
            Prefix = '$';
            Owners = new ulong[] { 0 };
            Token = "";
            BindToChannels = new ulong[] { 0 };
        }

        /// <summary> Save the current configuration object to a file. </summary>
        /// <param name="loc"> The configuration file's location. </param>
        public void SaveFile(string loc)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if (!File.Exists(loc))
                File.Create(loc).Close();

            File.WriteAllText(loc, json);
        }

        /// <summary> Load the information saved in your configuration file. </summary>
        /// <param name="loc"> The configuration file's location. </param>
        public static configuration LoadFile(string loc)
        {
            string json = File.ReadAllText(loc);
            return JsonConvert.DeserializeObject<configuration>(json);
        }
    }
}
