using Newtonsoft.Json;
using System.IO;

namespace discordMusicBot.src
{
    public class configuration
    {
        /// <summary> 
        /// Your bot's command prefix. Please don't pick `!`. 
        /// </summary>
        public char Prefix { get; set; }
        /// <summary> 
        /// Ids of users who will have owner access to the bot. 
        /// </summary>
        public ulong Owner { get; set; }
        /// <summary> 
        /// Your bot's login token. 
        /// </summary>
        public string Token { get; set; }
        /// <summary> 
        /// The ID of the room the bot should bind to on startup
        /// </summary>
        public ulong[] BindToChannels { get; set; }
        public ulong defaultRoomID { get; set; }
        /// <summary>
        ///     idDefaultGroup = id for @everyone
        /// </summary>
        public ulong idDefaultGroup { get; set; }
        /// <summary>
        ///     idModsGroup = id for bot mods,  Custom group
        /// </summary>
        public ulong idModsGroup { get; set; }
        /// <summary>
        ///     idAdminGroup = id for bot admins/owners, Custom group.
        /// </summary>
        public ulong idAdminGroup { get; set; }

        public int volume { get; set; }

        public configuration()
        {
            Prefix = '$';
            Owner = new ulong { };
            Token = "";
            BindToChannels = new ulong[] { 0 };
            defaultRoomID = 0;
            volume = 0;
            idDefaultGroup = new ulong { };
            idModsGroup = new ulong { };
            idAdminGroup = new ulong { };

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
