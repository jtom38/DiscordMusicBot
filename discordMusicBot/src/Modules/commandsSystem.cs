using System;
using System.Linq;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using discordMusicBot.src.Web;
using Discord;
using Discord.Commands;
using System.Reflection;
using Discord.WebSocket;

namespace discordMusicBot.src.Modules
{

    public class cmdSystem : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        private configuration _config;

        public cmdSystem(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        //configuration _config = new configuration();
        playlist _playlist = new playlist();
        system _system = new system();
        network _network = new network();
        logs _logs = new logs();
        discordStatus _discordStatus = new discordStatus();

        [Command("help")]
        public async Task HelpAsync(string command = null)
        {
            if (command == null)
            {
                char prefix = configuration.LoadFile().Prefix;
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = "These are the commands you can use"
                };

                foreach (var module in _service.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                            description += $"{prefix}{cmd.Aliases.First()}\n";
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                            x.IsInline = false;
                        });
                    }
                }

                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                var result = _service.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                char prefix = configuration.LoadFile().Prefix;
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Remarks: {cmd.Remarks}";
                        x.IsInline = false;
                    });
                }

                await ReplyAsync("", false, builder.Build());
            }

        }

        [Command("RemoveMessage")]
        [Remarks("Removes lines of messages from the current text channel.")]
        [Alias("rm")]
        public async Task RemoveMessageAsync(int counter = 0)
        {
            try
            {
                if (counter == 0)
                {
                    //await e.Channel.SendMessage($"{e.User.Name}, Please give me the number of lines you want to remove.");
                    var builder = new EmbedBuilder()
                    {
                        //unit error = uint.Parse(colors.Error)
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Title = $"{configuration.LoadFile().Prefix}RemoveMessage",
                        Description = $"{Context.User.Username},\rUnable to remove messages.  No number value was given."
                    };

                    await ReplyAsync("", false, builder.Build());
                    return;
                }

                int deleteCounter = 100;

                while (counter > 0)
                {
                    if (counter >= 100)
                    {
                        deleteCounter = 100;
                    }
                    else
                    {
                        deleteCounter = counter;
                    }

                    //IReadOnlyCollection<SocketMessage> msg = Context.Channel.GetCachedMessages(deleteCounter);

                    var msg =  Context.Message.Channel.GetMessagesAsync(100);
                    var msg1 = Context.Channel.GetCachedMessages(100);
                    var t = Context.Client.GetApplicationInfoAsync();

                    //tell server to download messages to memory
                    //Message[] messagesToDelete = await e.Channel.DownloadMessages(deleteCounter);

                    //tell bot to delete them from server
                    //await e.Channel.DeleteMessages(messagesToDelete);

                    counter = counter - 100;
                }

                //_logs.logMessage("Info", "commandsSystem.rm", $"User requested {e.GetArg("count")} lines to be removed from {e.Channel.Name}", e.User.Name);
            }        
            catch(Exception error)
            {
                //_logs.logMessage("Error", "commandsSystem.rm", error.ToString(), e.User.Name);
            }
        }

        [Command("Volume")]
        [Remarks("Sets the volume level of the audio player.")]
        [Alias("vol")]
        public async Task VolumeAsync(int userValue = 0)
        {
            try
            {
                if (userValue == 0)
                {

                    var builder = new EmbedBuilder()
                    {
                        //unit error = uint.Parse(colors.Error)
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Title = $"{configuration.LoadFile().Prefix}Volume",
                        Description = $"{Context.User.Username},\rPlease give me the percent value you want me to change to.\rExample: {configuration.LoadFile().Prefix}Volume 50"
                    };

                    await ReplyAsync("", false, builder.Build());
                    return;
                }


                if (userValue >= 1 && userValue <= 100)
                {
                    //convert the value that we deam a percent value to a string to format it
                    string stringVol = userValue.ToString();
                    string t = null;
                    if (stringVol.Length == 1)
                    {
                        t = $".0{stringVol}";
                    }
                    else
                    {
                        t = $".{stringVol}";
                    }


                    //convert to float
                    float newVol = float.Parse(t, System.Globalization.CultureInfo.InvariantCulture);

                    if (newVol >= 0f && newVol <= 1f)
                    {
                        //valid number
                        _config = configuration.LoadFile();
                        _config.volume = newVol;
                        _config.SaveFile();

                        player.volume = newVol; //send the updated value to the var so we dont have to load the config file everytime in the loop.

                        var builder = new EmbedBuilder()
                        {
                            //unit error = uint.Parse(colors.Error)
                            Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                            Title = $"{configuration.LoadFile().Prefix}Volume",
                            Description = $"{Context.User.Username},\rI have updated the volume to {userValue}%."
                        };

                        await ReplyAsync("", false, builder.Build());

                        //_logs.logMessage("Info", "commandsSyste.Volume", $"Volume was changed to {newVol}%", e.User.Name);
                    }
                }
                else if (userValue >= 101)
                {
                    var builder = new EmbedBuilder()
                    {
                        //unit error = uint.Parse(colors.Error)
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Title = $"{configuration.LoadFile().Prefix}Volume",
                        Description = $"{Context.User.Username},\rThe value you gave was higher then 100%, sorry."
                    };

                    await ReplyAsync("", false, builder.Build());
                }
                else if (userValue <= 0)
                {
                    var builder = new EmbedBuilder()
                    {
                        //unit error = uint.Parse(colors.Error)
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Title = $"{configuration.LoadFile().Prefix}Volume",
                        Description = $"{Context.User.Username},\rThe value can't go below 1, sorry."
                    };

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch
            {

            }
        }

        [Command("Shutdown")]
        [Remarks("Forces the bot to shutdown.")]
        public async Task HaltAsync()
        {
            try
            {
                if(Context.Guild.AudioClient != null) //if its null the AudioClient wasnt loaded/used so the bot isnt in a voice room.
                    await Context.Guild.AudioClient.DisconnectAsync();

                Environment.Exit(0);
            }
            catch (Exception error)
            {
                //_logs.logMessage("Error", "commandsSystem.halt", error.ToString(), e.User.Name);
            }
        }

        [Command("Restart")]
        [Remarks("Forces the bot to restart the application.")]
        public async Task RestartAsync()
        {
            try
            {
                //dump the current game playing
                await Context.Client.SetGameAsync(null);

                //send a message out on the restart
                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"{configuration.LoadFile().Prefix}Restart",
                    Description = $"{Context.User.Username},\rPlease wait as I reboot."
                };
                await ReplyAsync("", false, builder.Build());


                //check to see if she is in a voice room, if so disconnect 
                if (Context.Guild.AudioClient != null) //if its null the AudioClient wasnt loaded/used so the bot isnt in a voice room.
                    await Context.Guild.AudioClient.DisconnectAsync();

                //await _logs.logMessageAsync("Info", _config.Prefix + "restart", "Process was restarted by user", Context.User.Username);

                var fileName = Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(fileName);
                Environment.Exit(0);

            }
            catch (Exception error)
            {
                //await _logs.logMessageAsync("Error", "commandsSystem.restart", error.ToString(), Context.User.Username);
            }
        }

        [Command("ping")]
        [Remarks("Returns the current ping value of the bot.")]
        public async Task PingAsync()
        {
            try
            {

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"**{configuration.LoadFile().Prefix}Ping**",
                    Description = $"{Context.User.Username},\r**Datacenter**: {Context.Guild.VoiceRegionId}\r**Ping**: {Context.Guild.Discord.Latency}"
                };
                await ReplyAsync("", false, builder.Build());
            }
            catch
            {

            }
        }

    }
}
