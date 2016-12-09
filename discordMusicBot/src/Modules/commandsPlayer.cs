using Discord;
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
            logs _logs = new logs();
            
            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                //get the config file
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                _client.GetService<CommandService>().CreateCommand("test")
                    .Alias("test")
                    .Description("Placeholder for testing.")                
                    .Do(async e =>
                    {

                        //make var to store messages from the server
                        Message[] cacheMessages;

                        //tell server to download messages to memory
                        cacheMessages = await e.Channel.DownloadMessages(10);
                        //string url = null;

                        for(int i =0; i < 10; i++)
                        {
                            try
                            {
                                if (cacheMessages[i].Attachments[0].Url != null)
                                {
                                    var t = cacheMessages[i].Attachments;
                                }
                            }
                            catch
                            {
                                //discard error given I am aware this message has no attachment.
                            }                          
                        }

                        await e.Channel.SendMessage("test");
                    });
            
                _client.GetService<CommandService>().CreateCommand("play")
                    .Alias("play")
                    .Description("Adds the requested song to the queue.\rExample: !play url\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
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
                                await e.Channel.SendMessage($"Sorry the url you gave me was not a valid Youtube link.");
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
                            bool result = _player.cmd_skip();

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
                            bool result = _player.cmd_stop();

                            if (result == true)
                            {
                                _client.SetGame(null);
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
                            _player.cmd_resume();

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
                            Channel voiceChan = e.User.VoiceChannel;
                            await voiceChan.JoinAudio();
                            //await _playlist.startAutoPlayList(voiceChan, _client);
                            await _playlist.playAutoQueue(voiceChan, _client);
                            _logs.logMessage("Error", "commandsPlayer.summon", $"User has summoned the bot to room {voiceChan.ToString()}", e.User.Name);
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
