using System.Threading.Tasks;
using Discord.Commands;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using System;

namespace discordMusicBot.src.Modules
{
    public class cmdPlaylist : ModuleBase<SocketCommandContext>
    {

        private configuration _config;
        private CommandService _service;

        public cmdPlaylist(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        playlist _playlist = new playlist();
        player _player = new player();
        system _system = new system();
        youtube _downloader = new youtube();
        logs _logs = new logs();
        embed _embed = new embed();

        [Command("BlacklistAdd")]
        [Remarks("Adds a URL to the Blacklist file.")]
        [Alias("bla")]
        public async Task BlacklistAddAsync(string URL)
        {
            try
            {
                if (URL.Contains("https://www.youtube.com"))
                {
                    string title = await _system.cmd_blAdd(Context.User.Username, URL);

                    if (title == "dupe")
                    {
                        var builder = await _embed.SucessEmbedAsync("BlacklistAdd", $"I found this URL already in the list. :smile:\rNo change was made.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());
                    }
                    else
                    {
                        var builder = await _embed.SucessEmbedAsync("BlacklistAdd", $"**Title**: {title}\rHas been added to the blacklist file.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());

                        //send the infomation back to the user letting them know we added it to the blacklist.
                        await _logs.logMessageAsync("Info", "commandsPlaylist.blacklist add", $"Blacklist was updated. Added {title} {URL}", Context.User.Username);
                    }
                }
                else
                {
                    //error time
                    var builder = await _embed.ErrorEmbedAsync("BlacklistAdd", $"Please enter a valid URL.  I currently support Youtube only at this time.");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("BlacklistAdd");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}BlacklistAdd", error.ToString(), Context.User.Username);
            }
        }

        [Command("BlacklistRemove")]
        [Remarks("Removes a URL from the Blacklist file.")]
        [Alias("blr")]
        public async Task BlacklistRemoveAsync(string URL)
        {
            try
            {
                if (URL.Contains("https://www.youtube.com"))
                {
                    //parse the url and get the infomation then append to the blacklist.json
                    string url = await _system.cmd_blRemove(URL);

                    if (url == "match")
                    {
                        string[] urlTitle = await _downloader.returnYoutubeTitle(URL);

                        var builder = await _embed.SucessEmbedAsync("BlacklistRemove", $"**Title**: {urlTitle[0]}\rWas removed from the blacklist.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());

                        await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}BlacklistRemove", $"Blacklist was updated. Removed {urlTitle[0]} {URL}", Context.User.Username);
                    }
                    else
                    {
                        var builder = await _embed.SucessEmbedAsync("BlacklistRemove", $"Unable to find the song in the blacklist.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());
                    }
                }
                else
                {
                    //error time
                    var builder = await _embed.ErrorEmbedAsync("BlacklistRemove", $"Please enter a valid URL.  I currently support Youtube only at this time.");
                    await ReplyAsync("", false, builder.Build());
                }

            }
            catch (Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("BlacklistRemove");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}BlacklistRemove", error.ToString(), Context.User.Username);
            }
        }

        [Command("NowPlaying")]
        [Remarks("Returns the current track playing track.")]
        [Alias("np")]
        public async Task NowPlayingAsync()
        {
            try
            {
                string[] result = await _playlist.cmd_np();
                if (result[0] == null)
                {
                    var builder = await _embed.SucessEmbedAsync("NowPlaying", $"Sorry but a song is not currently playing.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    var builder = await _embed.SucessEmbedAsync("NowPlaying", $"Track currently playing\rTitle: {result[0]} \rURL: {result[1]}\rUser: {result[2]}\rSource: {result[3]}", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());

                    await _logs.logMessageAsync("Debug", $"{configuration.LoadFile().Prefix}NowPlaying", $"Now playing infomation was requested. Title: {result[0]} URL: {result[1]} User: {result[2]} Source: {result[3]} ", Context.User.Username);
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("NowPlaying");
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}NowPlaying", error.ToString(), Context.User.Username);
            }
        }

        [Command("NowPlayingRemove")]
        [Remarks("Removes the current playing track from the library.")]
        [Alias("npr")]
        public async Task NowPlayingRemoveAsync()
        {
            try
            {
                bool npRemoveResult = await _playlist.cmd_npRemove();
                if (npRemoveResult == true)
                {
                    await _player.cmd_skip();

                    var builder = await _embed.SucessEmbedAsync("ConfigureSmutRoom", $"The current playing track has been removed from the library as requested.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());

                    await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}NowPlayingRemove", $"URL: {playlist.npUrl} was removed from the Library", Context.User.Username);
                }
                else
                {
                    var builder = await _embed.ErrorEmbedAsync("RemoveMessage");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("NowPlayingRemove");
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}NowPlayingRemove", error.ToString(), Context.User.Username);
            }
        }
        
        [Command("Queue")]
        [Remarks("Displays the current queued up tracks")]
        public async Task QueueASync(int AmountToReturn = 5)
        {
            try
            {
                string result = null;

                if (AmountToReturn >= 20)
                {
                    AmountToReturn = 20;
                }
                result = await _playlist.cmd_queue(AmountToReturn);

                var builder = await _embed.SucessEmbedAsync("Queue", $"```{result}\r```", Context.User.Username);
                await ReplyAsync("", false, builder.Build());
                
            }
            catch (Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("Queue");
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Queue", error.ToString(), Context.User.Username);
            }
        }
        
        [Command("PlaylistAdd")]
        [Remarks("Adds a URL to the library file.")]
        [Alias("pla")]
        public async Task PlaylistAddAsync(string URL)
        {
            try
            {
                if (URL.Contains("https://www.youtube.com"))
                {
                    string title = await _system.cmd_plAdd(Context.User.Username, URL);

                    if (title == "dupe")
                    {
                        var builder = await _embed.SucessEmbedAsync("PlaylistAdd", $"I found this url already in the list. :smile:\rNo change was made.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());
                    }
                    else
                    {
                        var builder = await _embed.SucessEmbedAsync("PlaylistAdd", $"**Title**: {title}\rHas been added to the Playlist.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());

                        await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}PlaylistAdd", $"Playlist was updated. Added {title} {URL}", Context.User.Username);
                    }
                }
                else
                {
                    //error time
                    var builder = await _embed.ErrorEmbedAsync("PlaylistAdd", $"Please enter a valid URL.  I currently support Youtube only at this time.");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("PlaylistAdd");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}PlaylistAdd", error.ToString(), Context.User.Username);
            }
        }

        [Command("PlaylistRemove")]
        [Remarks("Removes a URL from the library file.")]
        [Alias("plr")]
        public async Task PlaylistRemoveAsync(string URL)
        {
            try
            {
                if (URL.Contains("https://www.youtube.com"))
                {
                    string url = await _system.cmd_plRemove(URL);

                    if (url == "match")
                    {
                        string[] urlTitle = await _downloader.returnYoutubeTitle(URL);

                        var builder = await _embed.SucessEmbedAsync("PlaylistRemove", $"**Title**: {urlTitle[0]}\rWas removed from the playlist.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());

                        await _logs.logMessageAsync("Info", "commandsPlaylist.playlist remove", $"Playlist was updated. Removed {urlTitle[0]} {URL}", Context.User.Username);
                    }
                    else
                    {
                        var builder = await _embed.SucessEmbedAsync("PlaylistRemove", $"Unable to find the song in the playlist.", Context.User.Username);
                        await ReplyAsync("", false, builder.Build());
                    }
                }
                else
                {
                    //error time
                    var builder = await _embed.ErrorEmbedAsync("PlaylistRemove", $"Please enter a valid URL.  I currently support Youtube only at this time.");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("PlaylistRemove");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}PlaylistRemove", error.ToString(), Context.User.Username);
            }
        }

        [Command("Shuffle")]
        [Remarks("Shuffles the items that users have submitted.")]
        public async Task ShuffleAsync()
        {
            try
            {
                string result = await _playlist.cmd_shuffle();

                if (result == "empty")
                {
                    var builder = await _embed.SucessEmbedAsync("Shuffle", $"No songs have been submitted to the queue so nothing to shuffle.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }

                if (result == "true")
                {
                    var builder = await _embed.SucessEmbedAsync("Shuffle", $"The queue has been shuffled!", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }

                if (result == "error")
                {
                    var builder = await _embed.ErrorEmbedAsync("Shuffle");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("Shuffle");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Shuffle", error.ToString(), Context.User.Username);
            }
        }

    }
}
