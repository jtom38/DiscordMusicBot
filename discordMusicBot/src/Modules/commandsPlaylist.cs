using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Modules;
using discordMusicBot.src;

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

            _config = configuration.LoadFile("config.json");

            manager.CreateCommands("", group =>
            {
                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plupdate")
                    .Alias("plupdate")
                    .Description("Goes out and fetches our google doc playlist file and updates the local copy.\r Permissions: Mods")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage($"Please wait... fetching the file");
                        //playlist _playlist = new playlist();
                        string responce = _playlist.updatePlaylistFile();
                        
                        await e.Channel.SendMessage(responce);
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "shuffle")
                    .Alias("shuffle")
                    .Description("Adds a url to the playlist file.\rPermissions: Everyone")
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
                    .Description("Returns infomation of curren playing track.\rPermissions: Everyone")
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

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plAdd")
                    .Alias("plAdd")
                    .Description("Adds a url to the playlist file.\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .Do(async e =>
                    {
                        if (e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to add to the playlist if you dont give me a url.");
                            return;
                        }

                        Message[] mess;

                        mess = await e.Channel.DownloadMessages(1);

                        await e.Channel.DeleteMessages(mess);
                        
                        string title = _playlist.cmd_plAdd(e.User.Name, e.GetArg("url"));

                        await e.Channel.SendMessage($"@{e.User.Name}\rTitle: " + title + "\rHas been added to the playlist file.");
                        Console.WriteLine($"{e.User.Name} added " + title + " to the playlist.json file.");
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "blAdd")
                    .Alias("blAdd")
                    .Description("Adds a url to the blacklist file.\rPermissions: Mods")
                    .Parameter("url", ParameterType.Optional)
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

                        //send the infomation back to the user letting them know we added it to the blacklist.
                        await e.Channel.SendMessage($"{e.User.Name}\rTitle: " + title + "\rHas been added to the blacklist file.");
                        Console.WriteLine($"{e.User.Name} added " + title + " to the blacklist.json file.");
                    });

            });
        }
    }
}
