using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using discordMusicBot.src.Web;
using Discord;
using Discord.Commands;
using System.IO;
using System.Reflection;
using Discord.WebSocket;

namespace discordMusicBot.src.Modules
{
    public interface IMessageChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
    {
        Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null);
    }

    public class commandsSystem : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        private configuration _config;

        public commandsSystem(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        //configuration _config = new configuration();
        playlist _playlist = new playlist();
        system _system = new system();
        network _network = new network();
        logs _logs = new logs();
        discordStatus _discordStatus = new discordStatus();

        

        [Command("RemoveMessage")]
        [Remarks("Query's UrbanDictionary.com for a random definition.")]
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
    }
}
