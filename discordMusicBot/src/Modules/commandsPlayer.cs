﻿using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Discord.Audio;
using System;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using discordMusicBot.src.Web;
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
            youtube _downloader = new youtube();
            player _player = new player();
            logs _logs = new logs();
            system _system = new system();
            
            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                //get the config file
                _config = configuration.LoadFile();

                _client.GetService<CommandService>().CreateCommand("test")
                    .Alias("test")
                    .Description("Placeholder for testing.")      
                    .Hide()        
                    .Do(async e =>
                    {
                        
                        await e.Channel.SendMessage($"test");
                    });
            
                _client.GetService<CommandService>().CreateCommand("play")
                    .Alias("play")
                    .Description("Adds the requested song to the queue.\rExample: !play url\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .Parameter("title", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.GetArg("url") == "")
                            {
                                await e.Channel.SendMessage($"{e.User.Mention}, Please give me a link so I can play the song for you.");
                                return;
                            }

                            //if return in 0 the user can add a track
                            int UserSubmit = await _playlist.checkNumberOfTracksByUserSubmitted(e.User.Name);
                            if(UserSubmit == 0)
                            {
                                //add the url to the listSubmitted 
                                if (e.GetArg("url").Contains("https://www.youtube.com/"))
                                {
                                    string result = await _playlist.cmd_play(e.GetArg("url"), e.User.Name);

                                    if (result == null)
                                    {
                                        await e.Channel.SendMessage($"Sorry I wont add that url to the queue given someone blacklisted it already.");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage(result);
                                        _logs.logMessage("Info", "commandsPlayer.play", $"URL:{e.GetArg("url")} was submitted to the queue.", e.User.Name);
                                    }
                                }
                                else
                                {
                                    switch (e.GetArg("url").ToLower())
                                    {
                                        case "title":
                                            string searchResult = await _playlist.cmd_searchLibrary(e.GetArg("url"), e.GetArg("title"), e.User.Name);


                                            if (searchResult == null)
                                            {
                                                await e.Channel.SendMessage($"Sorry I ran into a error.  Please check the log for more information.");
                                            }
                                            else if (searchResult.Contains("https://www.youtube.com/"))
                                            {
                                                string searchResultMessage = await _playlist.cmd_play(searchResult, e.User.Name);
                                                await e.Channel.SendMessage(searchResultMessage);
                                            }
                                            else
                                            {
                                                await e.Channel.SendMessage(searchResult);
                                            }
                                            break;
                                        default:
                                            await e.Channel.SendMessage($"{e.User.Name},\r Please enter a valid search mode.");
                                            break;
                                    }

                                }
                            }
                            else if(UserSubmit == 1)
                            {
                                await e.Channel.SendMessage($"{e.User.Name},\rYou have submitted too many tracks to the queue.  Please wait before you submit anymore.");
                            }

                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlayer.play", error.ToString(), e.User.Name);
                        }                      
                    });

                _client.GetService<CommandService>().CreateCommand("skip")
                    .Alias("skip")
                    .Description("Adds the requested song to the queue.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            bool result = await _player.cmd_skip();

                            if (result == true)
                            {
                                await e.Channel.SendMessage($"Skipping the track.");
                                _logs.logMessage("Info", "commandsPlayer.skip", "Track skip was requested.", e.User.Name);
                            }
                            else
                            {
                                await e.Channel.SendMessage($"Nothing is currently playing, unable to skip.");
                            }
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlayer.skip", error.ToString(), e.User.Name);
                        }                        
                    });

                _client.GetService<CommandService>().CreateCommand("stop")
                    .Alias("stop")
                    .Description("Stops the music from playing.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            Channel voice = null;
                            var bot = e.Server.FindUsers(_client.CurrentUser.Name).GetEnumerator();
                            while (bot.MoveNext())
                            {
                                if(bot.Current.Name == _client.CurrentUser.Name)//looking for the bot account
                                {
                                    voice = bot.Current.VoiceChannel;
                                }
                            }
                            
                            bool result = await _player.cmd_stop(_client, voice);

                            if (result == true)
                            {
                                _client.SetGame(null);
                                
                                //await e.Server.GetAudioClient;
                                await e.Channel.SendMessage($"Music will be stopping.");
                                _logs.logMessage("Info", "commandsPlayer.stop", "Player was requested to stop.", e.User.Name);

                            }
                            else
                            {
                                await e.Channel.SendMessage($"Nothing is currently playing, can't stop something that isnt moving.");
                            }
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlayer.stop", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("resume")
                    .Alias("resume")
                    .Description("Starts the playlist again.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            await _player.cmd_resume();

                            await _playlist.playAutoQueue(e.User.VoiceChannel, _client);

                            await e.Channel.SendMessage($"Activating the playlist again.");
                            _logs.logMessage("Info", "commandsPlayer.resume", "", e.User.Name);
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlayer.resume", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("summon")
                    .Alias("summon")
                    .Description("Summons bot to current voice channel and starts playing from the library.\rPermission: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {                       
                        try
                        {
                            //check to see if the bot is already in the room
                            //e.Server.FindUsers(_client.CurrentUser.Name).
                            //var bot = e.Server.FindUsers(_client.CurrentUser.Name);
                            //bot.GetEnumerator[0]
                            if (player.playingSong == false) // if the flag was set to not be okaying a song turn it on
                                player.playingSong = true;

                            if (playlist.playlistActive == false) //if the loop to keep playing tracks is off turn it on
                                playlist.playlistActive = true;

                            _logs.logMessage("Info", "commandsPlayer.summon", $"User has summoned the bot to room {e.User.VoiceChannel}", e.User.Name);

                            await _playlist.playAutoQueue(e.User.VoiceChannel, _client);
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlayer.summon", error.ToString(), e.User.Name);
                        }
                        
                    });

            });

        }
    }
}
