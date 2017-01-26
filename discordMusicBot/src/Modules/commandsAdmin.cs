using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discordMusicBot.src.sys;
using System;
using System.Collections.Generic;
using System.IO;
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

        system _system = new system();
        logs _logs = new logs();
        embed _embed = new embed();

        public cmdAdmin(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [Command("ConfigureGroupEveryone")]
        [Remarks("Configures what role ID gets Basic Permissions")]
        [Alias("cge")]
        public async Task ConfigureGroupEveryoneAsync(ulong UserValue)
        {
            try
            {
                _config = configuration.LoadFile();
                _config.idDefaultGroup = UserValue;
                _config.SaveFile();

                var builder = await _embed.SucessEmbedAsync("ConfigureGroupEveryone", $"Configuration Updated\r Everyone group is now linked to role: {UserValue}", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureGroupEveryone", $"User updated idDefaultGroup = {UserValue}", Context.User.Username);

            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ConfigureGroupEveryone", error.ToString(), Context.User.Username);

                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ConfigureGroupEveryone", error.ToString(), Context.User.Username);
            }
        }

        [Command("ConfigureGroupMods")]
        [Remarks("Configure what Role ID gets Mod Permissions")]
        [Alias("cgm")]
        public async Task ConfigureGroupModsAsync(ulong UserValue)
        {
            try
            {
                _config = configuration.LoadFile();
                _config.idModsGroup = UserValue;
                _config.SaveFile();

                var builder = await _embed.SucessEmbedAsync("ConfigureGroupMods", $"Configuration Updated\rMods group now linked to role: {UserValue}", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureGroupMods", $"Configuration Updated: Mods group now linked to role: {UserValue}", Context.User.Username);

            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ConfigureGroupMods", error.ToString(), Context.User.Username);

                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ConfigureGroupMods", error.ToString(), Context.User.Username);
            }
        }

        [Command("ConfigureGroupAdmins")]
        [Remarks("Configure what Role ID gets Mod Permissions")]
        [Alias("cga")]
        public async Task ConfigureGroupAdminsAsync(ulong UserValue)
        {
            try
            {
                _config = configuration.LoadFile();
                _config.idAdminGroup = UserValue;
                _config.SaveFile();

                var builder = await _embed.SucessEmbedAsync("ConfigureGroupAdmins", $"Configuration Updated\rAdmins group is now linked to role: {UserValue}", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureGroupAdmins", $"Configuration Updated: Admins group is now linked to role: {UserValue}", Context.User.Username);

            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ConfigureGroupAdmins", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ConfigureGroupAdmins", error.ToString(), Context.User.Username);
            }
        }

        [Command("ConfigureMaxSubmitted")]
        [Remarks("Configures max number of tracks a user can have in queue at a time.\rValue: 0 = Disabled\rValue: > 1 ")]
        [Alias("cms")]
        public async Task ConfigureMaxSubmittedAsync(int UserValue)
        {
            try
            {

                _config = configuration.LoadFile();
                _config.maxTrackSubmitted = UserValue;
                _config.SaveFile();

                string t = null;
                if (UserValue == 0)
                {
                    t = "Disabled";
                }
                else
                {
                    t = UserValue.ToString();
                }

                var builder = await _embed.SucessEmbedAsync("ConfigureMaxSubmitted", $"Configuration Updated\rNumber of Max Submitted Tracks = {t}.", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureMaxSubmitted", $"Configuration Updated: Number of Max Submitted Tracks = {t}.", Context.User.Username);
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ConfigureMaxSubmitted", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ConfigureMaxSubmitted", error.ToString(), Context.User.Username);
            }
        }

        [Command("ConfigureMusicRoom")]
        [Remarks("Defines what text channel music commands can be used")]
        [Alias("cmr")]
        public async Task ConfigureMusicRoom(ulong UserValue)
        {
            try
            {
                _config = configuration.LoadFile();
                _config.musicTextChannel = UserValue;
                _config.SaveFile();

                var builder = await _embed.SucessEmbedAsync("ConfigureMusicRoom", $"Configuration Updated\rMusic commands can now be only used in: {UserValue}.", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureMusicRoom", $"Configuration Updated: Smut is now only allowed in room: {UserValue}.", Context.User.Username);
            }
            catch
            {

            }
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
                    var builder = await _embed.ErrorEmbedAsync("ConfigurePrefix", "Please pick another command character that is not one of the following.\r'#' '/' '@'", Context.User.Username);
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

                    var builder = await _embed.SucessEmbedAsync("ConfigurePrefix", $"Configuration Updated\rPrefix is now: {UserValue}.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());

                    await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigurePrefix", $"Configuration Updated: Prefix is now: {UserValue}", Context.User.Username);
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ConfigurePrefix", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ConfigurePrefix", error.ToString(), Context.User.Username);
            }
        }

        [Command("ConfigureSmutRoom")]
        [Remarks("Defines where what text channel the command can be used.")]
        public async Task ConfigureSmutRoomAsync(ulong TextChannelID)
        {
            try
            {
                _config = configuration.LoadFile();
                _config.smutTextChannel = TextChannelID;
                _config.SaveFile();

                var builder = await _embed.SucessEmbedAsync("ConfigureSmutRoom", $"Configuration Updated\rSmut is now only allowed in room: {TextChannelID}.", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureSmutRoom", $"Configuration Updated: Smut is now only allowed in room: {TextChannelID}.", Context.User.Username);
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ConfigureSmutRoom", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ConfigureSmutRoom", error.ToString(), Context.User.Username);
            }
        }

        [Command("ExportPlayList")]
        [Remarks("Exports requested file to the user via a PM.")]
        [Alias("epl")]
        public async Task ExportPlaylistAsync()
        {
            try
            {
                bool plExport = await _system.cmd_plExport();

                if (plExport == true)
                {
                    string filePath = Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json";

                    var pm = await Context.User.CreateDMChannelAsync();
                    await pm.SendFileAsync(filePath, "Here is the file you requested");
                    await pm.CloseAsync();

                    File.Delete(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");

                    await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ExportPlaylist", "User requested the playlist file.", Context.User.Username);
                }
                else
                {
                    var builder = await _embed.ErrorEmbedAsync("ExportPlaylist", "Failed to generate the file.  Check the log for the dump.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ExportBlackList", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ExportPlaylist", error.ToString(), Context.User.Username);
            }
        }

        [Command("ExportBlackList")]
        [Remarks("Exports requested file to the user via a PM.")]
        [Alias("ebl")]
        public async Task ExportBlacklistAsync()
        {
            try
            {
                bool blExport = await _system.cmd_blExport();

                if (blExport == true)
                {
                    string filePath = Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json";

                    var pm = await Context.User.CreateDMChannelAsync();
                    await pm.SendFileAsync(filePath, "Here is the file you requested");
                    await pm.CloseAsync();

                    File.Delete(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");

                    await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ExportBlacklist", "User requested the Blacklist file.", Context.User.Username);
                }
                else
                {
                    
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ExportBlackList", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ExportBlacklist", error.ToString(), Context.User.Username);
            }
        }

        [Command("ExportLogs")]
        [Remarks("Exports requested file to the user via a PM.")]
        [Alias("el")]
        public async Task ExportLogAsync()
        {
            try
            {
                string filePath = Directory.GetCurrentDirectory() + "\\logs.txt";

                var pm = await Context.User.CreateDMChannelAsync();
                await pm.SendFileAsync(filePath, "Here is the file you requested");
                await pm.CloseAsync();

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ExportLogs", "User requested the Log file.", Context.User.Username);
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ExportLogs", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ExportLogs", error.ToString(), Context.User.Username);
            }

        }

        [Command("ExportConfig")]
        [Remarks("Exports requested file to the user via a PM.")]
        [Alias("ec")]
        public async Task ExportConfigAsync()
        {
            try
            {
                string filePath = Directory.GetCurrentDirectory() + "\\configs\\config.json";

                var pm = await Context.User.CreateDMChannelAsync();
                await pm.SendFileAsync(filePath, "Here is the file you requested");
                await pm.CloseAsync();

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ExportConfig", "User requested the Log file.", Context.User.Username);
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ExportConfig", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ExportConfig", error.ToString(), Context.User.Username);
            }

        }

        [Command("ServerIDS")]
        [Remarks("Returns the ulong ID values for all roles on the server.")]
        [Alias("sid")]
        public async Task ServerIDSAsync()
        {
            try
            {
                //extract the roles and id's from the server
                List<SocketRole> serverRolesList = Context.Guild.Roles.ToList();

                string result = null;
                for (int i = 0; i < serverRolesList.Count; i++)
                {
                    result = result + $"{serverRolesList[i].Name} = {serverRolesList[i].Id}\r";
                }

                var pm = await Context.User.CreateDMChannelAsync();

                var builder = await _embed.SucessEmbedAsync("ServerIDs", $"{result}", Context.User.Username);
                await ReplyAsync("", false, builder.Build());

                await pm.SendMessageAsync("", false, builder.Build());
                await pm.CloseAsync();

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ServerIDs", $"User has requested the Role IDs.", Context.User.Username);
            }
            catch (Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("ServerIDs", error.ToString(), Context.User.Username);
                await ReplyAsync("", false, builder.Build());
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}ServerIDs", error.ToString(), Context.User.Username);
            }
        }


    }
}
