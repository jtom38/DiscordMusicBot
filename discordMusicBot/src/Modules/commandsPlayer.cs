using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;

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
                    .Parameter("count", ParameterType.Optional)
                    .Do(async e =>
                    {
                        if(e.GetArg("count") == "")
                        {
                            await e.Channel.SendMessage($"@{e.User.Name}, Cant delete if you dont tell me how many to remove..");
                            return;
                        }
                        
                        //make var to store messages from the server
                        Message[] messagesToDelete;

                        //convert arg to int
                        int count = int.Parse(e.GetArg("count"));

                        //tell server to download messages to memory
                        messagesToDelete = await e.Channel.DownloadMessages(count);

                        //tell bot to delete them from server
                        await e.Channel.DeleteMessages(messagesToDelete);

                        //await e.Channel.SendMessage($"@{e.User.Name}, I have added {e.GetArg("url")} to autoplaylist.txt.");
                    });
            
                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "play")
                    .Alias("play")
                    .Description("Adds the requested song to the queue.")
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

                        /// <summary>
                        ///     File returns the following values currently 
                        ///     [0] Title
                        ///     [1] fileName
                        ///     [2] Full path to file
                        ///     [3] Bitrate
                        /// </summary>
                        string[] responce = _downloader.download_audio(e.GetArg("url"));

                        Channel voiceChan = e.User.VoiceChannel;

                        //await voiceChan.JoinAudio();

                        playingSong = true;

                        _client.SetGame("Playing " + responce[2]);
                        player _player = new player();
                        await _player.SendAudio(responce[2], voiceChan, playingSong, _client);

                        playingSong = false;
                        //await e.Channel.SendMessage($" @{e.User.Name} I have queued up " + responce +" for you. :smile:");
                    });

            });

        }
    }
}
