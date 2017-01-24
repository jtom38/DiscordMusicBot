﻿using Discord;
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

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"{configuration.LoadFile().Prefix}ConfigureGroupEveryone",
                    Description = $"{Context.User.Username},\rPermissions have been updated."
                };

                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}ConfigureGroupEveryone", $"User updated idDefaultGroup = {UserValue}", Context.User.Username);

            }
            catch(Exception error)
            {
                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                    Title = $"{configuration.LoadFile().Prefix}ConfigureGroupEveryone",
                    Description = $"{Context.User.Username},\r."
                };

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

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"{configuration.LoadFile().Prefix}ConfigureGroupMods",
                    Description = $"{Context.User.Username},\rPermissions have been updated."
                };

                await ReplyAsync("", false, builder.Build());

                //_logs.logMessage("Info", "commandsSystem.admin.setGroup default", $"Default role updated to {e.GetArg("id")}", e.User.Name);

            }
            catch
            {

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

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"{configuration.LoadFile().Prefix}ConfigureGroupAdmins",
                    Description = $"{Context.User.Username},\rPermissions have been updated."
                };

                await ReplyAsync("", false, builder.Build());

                //_logs.logMessage("Info", "commandsSystem.admin.setGroup default", $"Default role updated to {e.GetArg("id")}", e.User.Name);

            }
            catch
            {

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

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"{configuration.LoadFile().Prefix}ConfigureMaxSubmitted",
                    Description = $"{Context.User.Username},\rNumber of Max Submitted Tracks = {t}"
                };

                await ReplyAsync("", false, builder.Build());

                //await e.Channel.SendMessage($"{e.User.Name},\rI have disabled this function for you.");
                //_logs.logMessage("Info", "commandsSystem.admin.maxSubmitted", $"Max number of submitted tracks is now disabled", e.User.Name);

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

        [Command("ConfigureSmutRoom")]
        [Remarks("Defines where what text channel the command can be used.")]
        public async Task ConfigureSmutRoomAsync(ulong TextChannelID)
        {
            try
            {
                _config = configuration.LoadFile();
                _config.smutTextChannel = TextChannelID;
                _config.SaveFile();

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                    Title = $"{configuration.LoadFile().Prefix}ConfigurePrefix",
                    Description = $"{Context.User.Username},\rConfiguration Updated\rSmut is now only allowed in room: {TextChannelID}."
                };

                await ReplyAsync("", false, builder.Build());

                //logs.logMessage("Info", "commandsSystem.admin.setSmut", $"Smut Text Channel updated to {e.GetArg("id")}", e.User.Name);
            }
            catch
            {

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

                    //_logs.logMessage("Info", "commandsSystem.Export Playlist", "User requested the playlist file.", e.User.Name);
                }
                else
                {
                    //await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                }
            }
            catch
            {

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

                    //_logs.logMessage("Info", "commandsSystem.Export Playlist", "User requested the playlist file.", e.User.Name);
                }
                else
                {
                    //await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                }
            }
            catch
            {

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

                //_logs.logMessage("Info", "commandsSystem.Export Playlist", "User requested the playlist file.", e.User.Name);
            }
            catch
            {

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
            }
            catch
            {

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
                //List<Role> serverRolesList = e.Server.Roles.ToList();
                List<SocketRole> serverRolesList = Context.Guild.Roles.ToList();

                string result = null;
                for (int i = 0; i < serverRolesList.Count; i++)
                {
                    result = result + $"{serverRolesList[i].Name} = {serverRolesList[i].Id}\r";
                }

                var pm = await Context.User.CreateDMChannelAsync();

                var builder = new EmbedBuilder()
                {
                    Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                    Title = $"{configuration.LoadFile().Prefix}ServerIDS",
                    Description = $"{result}"
                };

                //await ReplyAsync("", false, builder.Build());

                await pm.SendMessageAsync("", false, builder.Build());
                await pm.CloseAsync();

                //await userPM.SendMessage($"```\r{result}\r```");

                //_logs.logMessage("Info", "commandsSystem.admin.serverIds", "User requested the server role IDs.", e.User.Name);
            }
            catch (Exception error)
            {
                //_logs.logMessage("Error", "commandsSystem.admin.serverIds", error.ToString(), e.User.Name);
            }
        }


    }
}
