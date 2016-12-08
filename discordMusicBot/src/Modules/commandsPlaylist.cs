using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using discordMusicBot.src;
using System.IO;

namespace discordMusicBot.src.Modules
{
    internal class commandsPlaylist : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private configuration _config;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            playlist _playlist = new playlist();
            system _system = new system();
            downloader _downloader = new downloader();
            logs _logs = new logs();

            _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

            manager.CreateCommands("", group =>
            {
                _client.GetService<CommandService>().CreateCommand("shuffle")
                    .Alias("shuffle")
                    .Description("Adds a url to the playlist file.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            string result = _playlist.cmd_shuffle();

                            if (result == "empty")
                            {
                                await e.Channel.SendMessage($"@{e.User.Name}\rNo songs have been submitted to be shuffled.");
                            }

                            if (result == "true")
                            {
                                await e.Channel.SendMessage($"@{e.User.Name}\rThe current queue has been shuffled.");
                            }

                            if (result == "error")
                            {
                                await e.Channel.SendMessage($"{e.User.Name}\rError please check the console for more information.");
                            }
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlaylist.shuffle", error.ToString(), e.User.Name);
                        }                     
                    });

                _client.GetService<CommandService>().CreateCommand("np")
                    .Alias("np")
                    .Description("Returns infomation of current playing track.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Parameter("flag",ParameterType.Optional)
                    .Do(async e =>
                    {
                        try
                        {
                            if(e.GetArg("flag") == null)
                            {
                                string[] result = _playlist.cmd_np();

                                if (result[0] == null)
                                {
                                    await e.Channel.SendMessage($"Sorry but a song is not currently playing.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage($"Track currently playing\rTitle: {result[0]} \rURL: {result[1]}\rUser: {result[2]}\rSource: {result[3]}");
                                    _logs.logMessage("Info", "commandsPlaylist.np", $"Now playing infomation was requested. Title: {result[0]} URL: {result[1]} User: {result[2]} Source: {result[3]} ", e.User.Name);
                                }
                            }
                            else
                            {
                                //going to remove a track from the playlist with what is currently playing.
                                bool npRemoveResult = _playlist.cmd_npRemove();
                                if(npRemoveResult == true)
                                {
                                    await e.Channel.SendMessage($"{e.User.Name}, the current playing track has been removed from the Library as requested.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage($"{e.User.Name}, I ran into a problem with your request.  Please see the log for more details.");
                                }
                            }
                            
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlaylist.np", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("queue")
                    .Alias("queue")
                    .Description("Returns infomation of currently queued tacks.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                            {
                            string result = _playlist.cmd_queue();

                            if (result == null)
                            {
                                await e.Channel.SendMessage($"Sorry nothing was submitted to the queue.");
                            }
                            else
                            {
                                await e.Channel.SendMessage($"Track currently playing\rTitle: " + result[0] + "\rURL: " + result[1] + "\rUser: " + result[2] + "\rSource: " + result[3]);
                            }
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlaylist.np", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("playlist")
                    .Alias("pl")
                    .Description("Adds a url to the playlist file.\rPermissions: Mods")
                    .Parameter("flag", ParameterType.Optional)
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.GetArg("url") == "")
                            {
                                await e.Channel.SendMessage($"@{e.User.Name}, Unable to adjust the playlist if you dont give me a url.");
                                return;
                            }

                            switch (e.GetArg("flag"))
                            {
                                case "add":
                                case "a":
                                    string title = await _system.cmd_plAdd(e.User.Name, e.GetArg("url"));

                                    if (title == "dupe")
                                    {
                                        await e.Channel.SendMessage($"{e.User.Name},\rI found this url already in the list. :smile:\rNo change was made.");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage($"{e.User.Name},\rTitle: {title}\rHas been added to the playlist file.");
                                        _logs.logMessage("Info", "commandsPlaylist.playlist add", $"Playlist was updated. Added {title} {e.GetArg("url")}", e.User.Name);
                                    }
                                    break;
                                case "remove":
                                case "r":
                                    string url = _system.cmd_plRemove(e.GetArg("url"));

                                    if (url == "match")
                                    {
                                        string urlTitle = await _downloader.returnYoutubeTitle(e.GetArg("url"));
                                        await e.Channel.SendMessage($"{e.User.Name},\rTitle: {urlTitle}\rWas removed from the playlist.");
                                        _logs.logMessage("Info", "commandsPlaylist.playlist remove", $"Playlist was updated. Removed {urlTitle} {e.GetArg("url")}", e.User.Name);
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage($"{e.User.Name},\rUnable to find the song in the playlist.");
                                    }

                                    break;
                                default:
                                    await e.Channel.SendMessage($"Invalid Argument\r{_config.Prefix}playlist add url\r{_config.Prefix}playlist remove url");
                                    break;
                            }

                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsPlaylist.playlist", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("blacklist")
                    .Alias("bl")
                    .Description("Adds a url to the blacklist file.\rPermissions: Mods")
                    .Parameter("flag", ParameterType.Optional)
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.GetArg("url") == "")
                            {
                                await e.Channel.SendMessage($"@{e.User.Name}, Unable to adjust the blacklist if you dont give me a url.");
                                return;
                            }
                            
                            switch (e.GetArg("flag"))
                            {
                                case "add":
                                    //parse the url and get the infomation then append to the blacklist.json
                                    string title = await _system.cmd_blAdd(e.User.Name, e.GetArg("url"));

                                    if (title == "dupe")
                                    {
                                        await e.Channel.SendMessage($"{e.User.Name},\rI found this url already in the list. :smile:\rNo change was made.");
                                    }
                                    else
                                    {
                                        //send the infomation back to the user letting them know we added it to the blacklist.
                                        await e.Channel.SendMessage($"{e.User.Name}\rTitle: {title}\rHas been added to the blacklist file.");
                                        _logs.logMessage("Info", "commandsPlaylist.blacklist add", $"Blacklist was updated. Added {title} {e.GetArg("url")}", e.User.Name);
                                    }
                                    break;
                                case "remove":
                                    //parse the url and get the infomation then append to the blacklist.json
                                    string url = _system.cmd_blRemove(e.GetArg("url"));

                                    if (url == "match")
                                    {
                                        string urlTitle = await _downloader.returnYoutubeTitle(e.GetArg("url"));
                                        await e.Channel.SendMessage($"{e.User.Name}\rTitle: {urlTitle}\rWas removed from the blacklist.");
                                        _logs.logMessage("Info", "commandsPlaylist.blacklist remove", $"Blacklist was updated. Removed {urlTitle} {e.GetArg("url")}", e.User.Name);
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage($"{e.User.Name}\rUnable to find the song in the blacklist.");
                                    }

                                    break;
                                default:
                                    await e.Channel.SendMessage($"Invalid Argument\r{_config.Prefix}blacklist add url\r{_config.Prefix}blacklist remove url");
                                    break;
                            }

                        }
                        catch (Exception error)
                        {
                                _logs.logMessage("Error", "commandsPlaylist.blacklist", error.ToString(), e.User.Name);
                        }
                    });

            });
        }
    }
}
