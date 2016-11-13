using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using Discord.Audio;

namespace discordMusicBot.src.Commands
{
    internal class CommandsModule : IModule
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

            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                //get the config file
                _config = configuration.LoadFile("config.json");

                _client.GetService<CommandService>().CreateCommand("test")
                    .Alias(_config.Prefix + "test")
                    .Description("Placeholder for testing.")
                    .Parameter("url", ParameterType.Optional)
                    .Do(async e =>
                    {
                        if(e.GetArg("url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Unable to add to the playlist if you dont give me a url.");
                            return;
                        }

                        _playlist.cmd_plAdd(e.User.Name, e.GetArg("url"));

                        await e.Channel.SendMessage($"@{e.User.Name}, I have added {e.GetArg("url")} to autoplaylist.txt.");
                    });

                
                _client.GetService<CommandService>().CreateCommand("play")
                    .Alias(_config.Prefix + "play")
                    .Description("Adds the requested song to the queue.")
                    .Parameter("play_url", ParameterType.Optional)
                    .Do(async e =>
                    {
                        if(e.GetArg("play_url") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Please give me a link so I can play the song for you.");
                            return;
                        }

                        //check to see if we are playing a song already
                        if(playingSong == true)
                        {
                            return;
                        }

                        downloader _downloader = new downloader();
                        string t = e.GetArg("play_url");

                        /// <summary>
                        ///     File returns the following values currently 
                        ///     [0] Title
                        ///     [1] fileName
                        ///     [2] Full path to file
                        ///     [3] Bitrate
                        /// </summary>
                        string[] responce = _downloader.download_audio(t);

                        Channel voiceChan = e.User.VoiceChannel;

                        //await voiceChan.JoinAudio();

                        playingSong = true;

                        _client.SetGame("Playing " + responce[2]);
                        player _player = new player();
                        await _player.SendAudio(responce[2], voiceChan, playingSong, _client);

                        playingSong = false;
                        //await e.Channel.SendMessage($" @{e.User.Name} I have queued up " + responce +" for you. :smile:");
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plupdate")
                    .Alias("plupdate")
                    .Description("Goes out and fetches our google doc playlist file and updates the local copy.")
                    .Do(async e =>
                    {
                        //await e.Channel.SendMessage($"Please wait... fetching the file");
                        //playlist _playlist = new playlist();
                        string responce = _playlist.updatePlaylistFile();

                        await e.Channel.SendMessage(responce);
                    });

                _client.GetService<CommandService>().CreateCommand("plAdd")
                    .Alias(_config.Prefix + "plAdd")
                    .Description("Adds a url to the playlist file.")
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
                    });


                _client.GetService<CommandService>().CreateCommand("summon")
                    .Alias(_config.Prefix + "summon")
                    .Description("Summons bot to current voice channel")
                    .Do(async e =>
                    {
                        Channel voiceChan = e.User.VoiceChannel;

                        await voiceChan.JoinAudio();
                    });


                _client.GetService<CommandService>().CreateCommand( "greet") //create command greet
                    .Alias(new string[] { "gr", "hi" }) //add 2 aliases, so it can be run with ~gr and ~hi
                    .Description("Greets a person.") //add description, it will be shown when ~help is used
                    .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
                        //sends a message to channel with the given text
                    });


            });

        }
    }
}
