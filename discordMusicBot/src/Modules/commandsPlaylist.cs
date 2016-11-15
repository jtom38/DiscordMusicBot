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
                    .Description("Goes out and fetches our google doc playlist file and updates the local copy.")
                    .Description("Permissions: Mods")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage($"Please wait... fetching the file");
                        //playlist _playlist = new playlist();
                        string responce = _playlist.updatePlaylistFile();
                        
                        await e.Channel.SendMessage(responce);
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plAdd")
                    .Alias("plAdd")
                    .Description("Adds a url to the playlist file.")
                    .Description("Permissions: Mods")
                    .Parameter("url", ParameterType.Optional)
                    .Do(async e =>
                    {
                        if (e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to add to the playlist if you dont give me a url.");
                            return;
                        }

                        string title = _playlist.cmd_plAdd(e.User.Name, e.GetArg("url"));

                        await e.Channel.SendMessage($"@{e.User.Name}, I have added " + title + " to the playlist file.");
                        Console.WriteLine($"{e.User.Name} added " + title + " to the playlist.json file.");
                    });

            });
        }
    }
}
