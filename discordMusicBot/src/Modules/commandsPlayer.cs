﻿using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Discord.Audio;
using System;
using System.Threading.Tasks;
using discordMusicBot.src;
using System.IO;
using System.Collections.Generic;

namespace discordMusicBot.src.Modules
{
    internal class commandsPlayer : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private configuration _config;
        

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            playlist _playlist = new playlist();
            downloader _downloader = new downloader();
            player _player = new player();
            
            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                //get the config file
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "test")
                    .Alias("test")
                    .Description("Placeholder for testing.")                  
                    .Do(async e =>
                    {
                        //string[] result = _playlist.cmd_np();

                        Channel userPM = await e.User.CreatePMChannel();

                        await userPM.SendMessage("test");


                        //await e.Channel.SendMessage("");
                    });
            
                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "play")
                    .Alias("play")
                    .Description("Adds the requested song to the queue.\rExample: !play url\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        if(e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Mention}, Please give me a link so I can play the song for you.");
                            return;
                        }

                        //add the url to the listSubmitted 
                        string result = _playlist.cmd_play(e.GetArg("url"), e.User.Name);

                        await e.Channel.SendMessage(result);

                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "skip")
                    .Alias("skip")
                    .Description("Adds the requested song to the queue.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        bool result = _player.cmd_skip();

                        if(result == true)
                        {
                            await e.Channel.SendMessage($"Skipping the track.");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"Nothing is currently playing, unable to skip.");
                        }
                        
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "stop")
                    .Alias("stop")
                    .Description("Stops the music from playing.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        bool result = _player.cmd_stop();

                        if (result == true)
                        {
                            _client.SetGame("");
                            await e.Channel.SendMessage($"Music will be stopping.");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"Nothing is currently playing, can't stop something that isnt moving.");
                        }

                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "resume")
                    .Alias("resume")
                    .Description("Starts the playlist again.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        //await _playlist.startAutoPlayList();

                        await e.Channel.SendMessage($"Activating the playlist again.");

                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "summon")
                    .Alias("summon")
                    .Description("Summons bot to current voice channel and starts playing from the library.\rPermission: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
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
