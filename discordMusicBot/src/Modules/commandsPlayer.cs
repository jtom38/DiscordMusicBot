using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using Discord.Audio;
using System;

namespace discordMusicBot.src.Modules
{
    internal class commandsPlayer : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private configuration _config;

        private bool playingSong = false;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;
            playlist _playlist = new playlist();
            downloader _downloader = new downloader();
            
            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                //get the config file
                _config = configuration.LoadFile("config.json");

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "test")
                    .Alias("test")
                    .Description("Placeholder for testing.")                   
                    .Do(async e =>
                    {
                        string[] result = _playlist.cmd_np();

                        await e.Channel.SendMessage("placeholder.");
                    });
            
                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "play")
                    .Alias("play")
                    .Description("Adds the requested song to the queue.\rExample: !play url\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .Do(async e =>
                    {
                        if(e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Mention}, Please give me a link so I can play the song for you.");
                            return;
                        }

                        //check to see if we are playing a song already
                        if(playingSong == true)
                        {
                            return;
                        }

                        //0 = default
                        Channel voiceChan = e.Server.GetChannel(0);
                        if (_config.defaultRoomID == 0)
                        {
                            voiceChan = e.Server.GetChannel(_config.defaultRoomID);
                            await voiceChan.JoinAudio();
                        }
                        else
                        {
                            voiceChan = e.User.VoiceChannel;
                            await voiceChan.JoinAudio();
                        }

                        /// <summary>
                        ///     File returns the following values currently 
                        ///     [0] Title
                        ///     [1] fileName
                        ///     [2] Full path to file
                        ///     [3] Bitrate
                        /// </summary>

                        string[] responce = _downloader.download_audio(e.GetArg("url"));

                        playingSong = true;

                        _client.SetGame("Playing " + responce[2]);
                        player _player = new player();
                        await _player.SendAudio(responce[2], voiceChan, playingSong, _client);

                        playingSong = false;
                        //await e.Channel.SendMessage($" @{e.User.Name} I have queued up " + responce +" for you. :smile:");
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "summon")
                    .Alias("summon")
                    .Description("Summons bot to current voice channel and starts playing from the library.\rPermission: Everyone")
                    .Do(async e =>
                    {
                        //0 = default
                        Channel voiceChan = e.Server.GetChannel(0);
                        if (_config.defaultRoomID != 0)
                        {
                            voiceChan = e.Server.GetChannel(_config.defaultRoomID);
                            await voiceChan.JoinAudio();
                        }
                        else
                        {
                            voiceChan = e.User.VoiceChannel;
                            await voiceChan.JoinAudio();
                        }

                        try
                        {
                            await _playlist.startAutoPlayList(voiceChan, _client);
                        }
                        catch(Exception t)
                        {
                            Console.WriteLine(t);
                        }
                        
                    });

            });

        }
    }
}
