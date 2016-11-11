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

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                //get the config file
                _config = configuration.LoadFile("config.json");

                _client.GetService<CommandService>().CreateCommand("test")
                    .Alias(_config.Prefix + "test")
                    .Description("Placeholder for testing.")
                    .Do(async e =>
                    {
                        downloader _download = new downloader();
                        string[] file = _download.download_audio("https://www.youtube.com/watch?v=oY9m2sHQwLs");

                        /// <summary>
                        ///     File returns the following values currently 
                        ///     [0] Title
                        ///     [1] fileName
                        ///     [2] Full path to file
                        ///     [3] Bitrate
                        /// </summary>

                        await e.Channel.SendMessage($"Test: " + file);
                    });

                
                _client.GetService<CommandService>().CreateCommand("play")
                    .Alias(_config.Prefix + "play")
                    .Description("Adds the requested song to the queue.")
                    .Parameter("play_url", ParameterType.Required)
                    .Do(async e =>
                    {
                        downloader _downloader = new downloader();
                        string t = e.GetArg("play_url");
                        string[] responce = _downloader.download_audio(t);
                        await e.Channel.SendMessage($" {e.User.Name} I have queued up " + responce +" for you. :smile:");
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plupdate")
                    .Alias("plupdate")
                    .Description("Goes out and fetches our google doc playlist file and updates the local copy.")
                    .Do(async e =>
                    {
                        //await e.Channel.SendMessage($"Please wait... fetching the file");
                        playlist _playlist = new playlist();
                        string responce = _playlist.updatePlaylistFile();

                        await e.Channel.SendMessage(responce);
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
