using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using System.IO;

namespace discordMusicBot.src.Modules
{
    public class commandsPlaylist : ModuleBase<SocketCommandContext>
    {

        private configuration _config;
        private CommandService _service;

        public commandsPlaylist(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        playlist _playlist = new playlist();
            player _player = new player();
            system _system = new system();
            youtube _downloader = new youtube();
            logs _logs = new logs();


        
    }
}
