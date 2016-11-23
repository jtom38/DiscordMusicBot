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

            _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

            manager.CreateCommands("", group =>
            {
                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "shuffle")
                    .Alias("shuffle")
                    .Description("Adds a url to the playlist file.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        string result = _playlist.cmd_shuffle();

                        if(result == "empty")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}\rNo songs have been submitted to be shuffled.");
                        }

                        if(result == "true")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}\rThe current queue has been shuffled.");
                        }

                        if(result == "error")
                        {
                            await e.Channel.SendMessage($"{e.User.Name}\rError please check the console for more information.");
                        }
                        
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "np")
                    .Alias("np")
                    .Description("Returns infomation of current playing track.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        string[] result = _playlist.cmd_np();
                        
                        if(result[0] == null)
                        {
                            await e.Channel.SendMessage($"Sorry but a song is not currently playing.");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"Track currently playing\rTitle: " + result[0] + "\rURL: " + result[1] + "\rUser: " + result[2] + "\rSource: " + result[3]);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "queue")
                    .Alias("queue")
                    .Description("Returns infomation of currently queued tacks.\rPermissions: Everyone")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
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
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plAdd")
                    .Alias(new string[] { "plAdd", "pla" })
                    .Description("Adds a url to the playlist file.\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        if (e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to add to the playlist if you dont give me a url.");
                            return;
                        }
                        
                        Message[] mess = await e.Channel.DownloadMessages(1);
                        await e.Channel.DeleteMessages(mess);
                        
                        string title = _playlist.cmd_plAdd(e.User.Name, e.GetArg("url"));

                        if (title == "dupe")
                        {
                            await e.Channel.SendMessage($"{e.User.Name},\rI found this url already in the list. :smile:\rNo change was made.");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"{e.User.Name},\rTitle: {title}\rHas been added to the playlist file.");
                            Console.WriteLine($"{e.User.Name} added {title} to the playlist.json file.");
                        }
                        
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plRemove")
                    .Alias(new string[] { "plRemove", "plr" })
                    .Description("Removes a url to the playlist file.\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        if (e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to remove the url from the playlist if you dont give me a url.");
                            return;
                        }

                        Message[] mess = await e.Channel.DownloadMessages(1);
                        await e.Channel.DeleteMessages(mess);

                        string url = _playlist.cmd_plRemove(e.GetArg("url"));

                        if (url == "match")
                        {
                            downloader _downloader = new downloader();
                            string title = _downloader.returnYoutubeTitle(e.GetArg("url"));
                            await e.Channel.SendMessage($"{e.User.Name},\rTitle: {title}\rWas removed from the playlist.");
                            Console.WriteLine($"{e.User.Name} removed {title} from the playlist.json file.");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"{e.User.Name},\rUnable to find the song in the playlist.");
                        }

                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "blAdd")
                    .Alias(new string[] { "blAdd", "bla"})
                    .Description("Adds a url to the blacklist file.\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        if (e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to add to the blacklist if you dont give me a url.");
                            return;
                        }

                        //make the var we will store the messages in from the server
                        Message[] mess = await e.Channel.DownloadMessages(1);
                        //send delete command.  THis will delete the message that the user sent with the url
                        await e.Channel.DeleteMessages(mess);
                        //parse the url and get the infomation then append to the blacklist.json
                        string title = _playlist.cmd_blAdd(e.User.Name, e.GetArg("url"));

                        if(title == "dupe")
                        {
                            await e.Channel.SendMessage($"{e.User.Name},\rI found this url already in the list. :smile:\rNo change was made.");
                        }
                        else
                        {
                            //send the infomation back to the user letting them know we added it to the blacklist.
                            await e.Channel.SendMessage($"{e.User.Name}\rTitle: " + title + "\rHas been added to the blacklist file.");
                            Console.WriteLine($"{e.User.Name} added " + title + " to the blacklist.json file.");
                        }
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "blRemove")
                    .Alias(new string[] { "blRemove", "blr" })
                    .Description("Removes a url to the blacklist file.\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        if (e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to remove the url from the blacklist if you dont give me a url.");
                            return;
                        }

                        Message[] mess = await e.Channel.DownloadMessages(1);
                        await e.Channel.DeleteMessages(mess);

                        string url = _playlist.cmd_blRemove(e.GetArg("url"));

                        if (url == "match")
                        {
                            downloader _downloader = new downloader();
                            string title = _downloader.returnYoutubeTitle(e.GetArg("url"));
                            await e.Channel.SendMessage($"{e.User.Name},\rTitle: {title}\rWas removed from the blacklist.");
                            Console.WriteLine($"{e.User.Name} removed {title} from the blacklist.json file.");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"{e.User.Name},\rUnable to find the song in the blacklist.");
                        }

                    });

            });
        }
    }
}
