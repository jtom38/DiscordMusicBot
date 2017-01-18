using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using discordMusicBot.src.Web;
using Discord;
using Discord.Commands;
using System.IO;
using System.Reflection;

namespace discordMusicBot.src.Modules
{
    public class commandsSystem : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;
        private configuration _config;

        public commandsSystem(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        playlist _playlist = new playlist();
        system _system = new system();
        network _network = new network();
        logs _logs = new logs();
        discordStatus _discordStatus = new discordStatus();


    }
}
