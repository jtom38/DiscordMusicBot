using Discord.Commands;
using System;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace discordMusicBot.src.Modules
{
    public class cmdPlayer : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;
        private configuration _config;
        private DiscordSocketClient _client;

        public cmdPlayer(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        playlist _playlist = new playlist();
        youtube _downloader = new youtube();
        player _player = new player();
        logs _logs = new logs();
        system _system = new system();
        embed _embed = new embed();

        [Command("Play")]
        [Remarks("")]
        [Alias("p")]
        public async Task PlayAsync(string URL, string title)
        {
            try
            {
                //if return in 0 the user can add a track
                int UserSubmit = await _playlist.checkNumberOfTracksByUserSubmitted(Context.User.Username);
                if (UserSubmit == 0)
                {
                    //add the url to the listSubmitted 
                    if (URL.Contains("https://www.youtube.com/"))
                    {
                        string[] result = await _playlist.cmd_play(URL, Context.User.Username);

                        if (result == null)
                        {
                            var builder = await _embed.SucessEmbedAsync("Play", $"Sorry I wont add that url to the queue given someone blacklisted it already.", Context.User.Username);
                            await ReplyAsync("", false, builder.Build());
                        }
                        else
                        {
                            var builder = await _embed.SucessEmbedAsync("Play", $"Your track request of {result[0]} has been submitted.\rTracks in queue: {result[1]}", Context.User.Username);
                            await ReplyAsync("", false, builder.Build());

                            await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}Play", $"URL:{URL} was submitted to the queue.", Context.User.Username);
                        }
                    }
                    else
                    {
                        switch (URL.ToLower())
                        {
                            case "title":
                                string searchResult = await _playlist.cmd_searchLibrary(URL, title );

                                if (searchResult == null)
                                {
                                    var builder1 = await _embed.ErrorEmbedAsync("Title");
                                    await ReplyAsync("", false, builder1.Build());
                                }
                                else if (searchResult.Contains("https://www.youtube.com/"))
                                {
                                    string[] searchResultMessage = await _playlist.cmd_play(searchResult, Context.User.Username);

                                    var builder2 = await _embed.SucessEmbedAsync("Play", $"Your track request of {searchResultMessage[0]} has been submitted.\rTracks in queue: {searchResultMessage[1]}", Context.User.Username);
                                    await ReplyAsync("", false, builder2.Build());
                                }
                                else
                                {
                                    var builder3 = await _embed.ErrorEmbedAsync("Play");
                                    await ReplyAsync("", false, builder3.Build());

                                }
                                break;
                            default:
                                var builder = await _embed.ErrorEmbedAsync("Play", "Please enter a valid search mode.");
                                await ReplyAsync("", false, builder.Build());

                                break;
                        }
                    }
                }
                else if (UserSubmit == 1)
                {
                    var builder = await _embed.ErrorEmbedAsync("Play", "You have submitted too many tracks to the queue.  Please wait before you submit anymore.");
                    await ReplyAsync("", false, builder.Build());

                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("Play");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Play", error.ToString(), Context.User.Username);
            }
        }

        [Command("Skip")]
        [Remarks("Skips the curent playing track.")]
        public async Task SkipAsync()
        {
            try
            {
                bool result = await _player.cmd_skip();

                if (result == true)
                {
                    var builder = await _embed.SucessEmbedAsync("Skip", "Skiping the current track.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());

                    await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}Skip", "Track skip was requested.", Context.User.Username);
                }
                else
                {
                    var builder = await _embed.ErrorEmbedAsync("Skip", $"Nothing is currently playing, unable to skip.");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("Skip");
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Skip", error.ToString(), Context.User.Username);
            }
        }

        [Command("Stop")]
        [Remarks("Stops the audio player.")]
        public async Task StopAsync()
        {
            try
            {
                var voice = Context.Guild.CurrentUser.VoiceChannel;

                //var bot = e.Server.FindUsers(_client.CurrentUser.Name).GetEnumerator();
                //while (bot.MoveNext())
                //{
                    //if (bot.Current.Name == _client.CurrentUser.Name)//looking for the bot account
                    //{
                        //voice = bot.Current.VoiceChannel;
                    //}
                //}

                bool result = await _player.cmd_stop();

                if (result == true)
                {
                    await _client.SetGameAsync($"Type {configuration.LoadFile().Prefix}help for help");

                    var builder = await _embed.SucessEmbedAsync("Stop", "Skipping the current track.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());

                    await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}Stop", "Player was requested to stop.", Context.User.Username);
                }
                else
                {
                    var builder = await _embed.ErrorEmbedAsync("Stop", "Nothing is currently playing, can't stop something that isnt moving.");
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Stop", error.ToString(), Context.User.Username);
            }
        }

        [Command("Resume")]
        [Remarks("Resumes the audio player if the audio was paused.")]
        public async Task ResumeAsync()
        {
            try
            {
                await _player.cmd_resume();

                //await _playlist.playAutoQueue(Context.Guild.CurrentUser.VoiceChannel, _client);

                var builder = await _embed.SucessEmbedAsync("Resume", "Activating the playlist again.", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}Resume", "", Context.User.Username);
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync ("Error", $"{configuration.LoadFile().Prefix}Resume", error.ToString(), Context.User.Username);
            }
        }

        [Command("Summon")]
        [Remarks("Summons the bot to a voice room.")]
        public async Task SummonAsync()
        {
            try
            {
                //check to see if the bot is already in the room
                if (player.playingSong == false) // if the flag was set to not be okaying a song turn it on
                    player.playingSong = true;

                if (playlist.playlistActive == false) //if the loop to keep playing tracks is off turn it on
                    playlist.playlistActive = true;

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}Summon", $"User has summoned the bot to room {Context.Guild.CurrentUser.VoiceChannel}", Context.User.Username);

                var msg =  Context.Message.Channel;

                await _playlist.playAutoQueue(e.User.VoiceChannel, _client);
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Summon", error.ToString(), Context.User.Username);
            }
        }
    }
}
