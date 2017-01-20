using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discordMusicBot.src.sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discordMusicBot.src.Modules
{
    public class cmdAdmin : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        private configuration _config;

        public cmdAdmin(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [Command("ConfigurePrefix")]
        [Remarks("Sets the prefix that the bot will respond to.")]
        [Alias("prefix", "cp")]
        public async Task ConfigPrefixAsync(string UserValue)
        {
            try
            {
                if (UserValue == "#" ||
                    UserValue == "/" ||
                    UserValue == "@")
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Title = $"{configuration.LoadFile().Prefix}ConfigurePrefix",
                        Description = $"{Context.User.Username},\rPlease pick another command character that is not one of the following.\r'#' '/' '@'"
                    };

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                else
                {
                    var game = $"Type {configuration.LoadFile().Prefix}help for help";
                    if (Context.User.Game.Value.ToString() == game)
                    {
                        await Context.Client.SetGameAsync($"Type {UserValue[0]}help for help");
                    }

                    _config = configuration.LoadFile();
                    _config.Prefix = UserValue[0];
                    _config.SaveFile();

                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                        Title = $"{configuration.LoadFile().Prefix}ConfigurePrefix",
                        Description = $"{Context.User.Username},\rCharacter prefix has been changed to {UserValue} and will be active on next restart."
                    };

                    await ReplyAsync("", false, builder.Build());
                    
                    //_logs.logMessage("Info", "commandsSystem.admin.setPrefix", $"Commands prefix was changed to {e.GetArg("value")}.", e.User.Name);
                }
            }
            catch
            {

            }
        }

    }

    [Group("ConfigSmut")]
    public class ConfigSmut : ModuleBase
    {
        [Command("SmutID")]
        [Remarks("Defines where what text channel the command can be used.")]
        public async Task SetSmutAsync(string mode)
        {

        }
    }
}
