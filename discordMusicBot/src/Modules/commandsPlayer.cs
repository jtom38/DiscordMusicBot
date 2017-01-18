using Discord.Commands;
using System;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using System.Collections.Generic;

namespace discordMusicBot.src.Modules
{
    internal class commandsPlayer : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;
        private configuration _config;

        public commandsPlayer(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }


        playlist _playlist = new playlist();
            youtube _downloader = new youtube();
            player _player = new player();
            logs _logs = new logs();
            system _system = new system();
            


        
    }
}
