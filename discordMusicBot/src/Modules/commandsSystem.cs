using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Audio;
using Discord.Modules;
using System.IO;
using System.Reflection;

namespace discordMusicBot.src.Modules
{
    internal class commandsSystem : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private configuration _config;

        playlist _playlist = new playlist();
        system _system = new system();
        network _network = new network();
        logs _logs = new logs();

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

            manager.CreateCommands("", group =>
            {
                _client.GetService<CommandService>().CreateCommand("rm")
                    .Alias("rm")
                    .Description("Removes messages from a text channel.\rExample: !rm 100\rPermissions: Everyone")
                    .Parameter("count", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.GetArg("count") == "")
                            {
                                await e.Channel.SendMessage($"{e.User.Name}, Please give me the number of lines you want to remove.");
                                return;
                            }

                            //convert the arg from string to int
                            int counter = int.Parse(e.GetArg("count"));
                            int deleteCounter = 100;

                            while(counter > 0)
                            {
                                if(counter >= 100)
                                {
                                    deleteCounter = 100;
                                }
                                else
                                {
                                    deleteCounter = counter;
                                }

                                //tell server to download messages to memory
                                Message[] messagesToDelete = await e.Channel.DownloadMessages(deleteCounter);

                                //tell bot to delete them from server
                                await e.Channel.DeleteMessages(messagesToDelete);

                                counter = counter - 100;
                            }

                            _logs.logMessage("Info", "commandsSystem.rm", $"User requested {e.GetArg("count")} lines to be removed from {e.Channel.Name}", e.User.Name);

                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.rm", error.ToString(), e.User.Name);
                        }

                    });

                _client.GetService<CommandService>().CreateCommand("defaultRoom")
                    .Alias("defaultRoom")
                    .Description("Sets the bots default voice room.\rExample: !defaultRoom roomID\rPermission: Owner")
                    .Parameter("roomID", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.GetArg("roomID") == null)
                            {
                                await e.Channel.SendMessage("Oops, you forgot to give me the room ID to make my home by default.");
                                return;
                            }

                            ulong id = Convert.ToUInt64(e.GetArg("roomID"));

                            _config.defaultRoomID = id;
                            _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                            await e.Channel.SendMessage("I have updated the config file for you.");
                            _logs.logMessage("Info", "commandsSystem.defaultRoom", $"defaultRoom was updated to {e.GetArg("roomID")}", e.User.Name);

                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.defaultRoom", error.ToString(), e.User.Name);
                        }

                    });

                _client.GetService<CommandService>().CreateCommand("vol")
                    .Alias("vol")
                    .Description("Adjusts the default volume from the bot.\rExample: !vol +10\rPermission: Everyone")
                    .Parameter("vol", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.GetArg("vol") == null)
                            {
                                await e.Channel.SendMessage("Oops, you forgot to give me the room ID to make my home by default.");
                                return;
                            }

                            // v is the current value in the config
                            int oldVolume = _config.volume;

                            //
                            string argOperator = null;
                            int argValue = 0;
                            int newValue = 0;

                            //will capture a + or -
                            try
                            {
                                argOperator = e.GetArg("vol").Substring(0, 1);
                                if (argOperator != "+" || argOperator != "-")
                                {
                                    //return error if we dont have a + or -
                                    return;
                                }
                            }
                            catch
                            {
                                Console.WriteLine("");
                            }

                            //parse the int value
                            try
                            {
                                //get the number value
                                string value = e.GetArg("vol").Substring(1);
                                int.Parse(value);
                            }
                            catch
                            {
                                //failed to parse the int value from arg
                                return;
                            }

                            if (argOperator == "+")
                            {
                                newValue = oldVolume + argValue;
                            }

                            if (argOperator == "-")
                            {
                                newValue = oldVolume - argValue;
                            }

                            _config.volume = newValue;
                            _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                            await e.Channel.SendMessage($"{e.User.Name} changed default volume from " + oldVolume + " to " + newValue + ".");
                            _logs.logMessage("Info", "commandsSystem.vol", $"Vol was changed from {oldVolume} to {newValue}", e.User.Name);
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.vol", error.ToString(), e.User.Name);
                        }

                    });

                _client.GetService<CommandService>().CreateCommand("halt")
                    .Alias("halt")
                    .Description("Shuts down the bot.\rPermission: Owner")
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            await e.Channel.SendMessage($":wave: :zzz:");

                            _logs.logMessage("Info", "commandsSystem.halt", $"Server was requested to shutdown.", e.User.Name);

                            await e.Server.Client.Disconnect();

                            Environment.Exit(0);
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.halt", error.ToString(), e.User.Name);
                        }

                    });

                _client.GetService<CommandService>().CreateCommand("restart")
                    .Alias("reboot")
                    .Description("Shuts down the bot.\rPermission: Mods")
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        try
                        {
                            //dump the current game playing
                            _client.SetGame(null);

                            //send a message out on the restart
                            await e.Channel.SendMessage($":wave: :zzz:");

                            //check to see if she is in a voice room, if so disconnect 
                            var bot = e.Server.FindUsers(_client.CurrentUser.Name).FirstOrDefault().VoiceChannel;

                            //check to see if the bot is in a room.
                            if (bot != null)
                            {
                                //if she is, disconnect from the room.
                                await bot.LeaveAudio();
                            }

                            _logs.logMessage("Info", _config.Prefix + "restart", "Process was restarted by user", e.User.Name);

                            var fileName = Assembly.GetExecutingAssembly().Location;
                            System.Diagnostics.Process.Start(fileName);
                            Environment.Exit(0);

                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.restart", error.ToString(), e.User.Name);
                        }

                    });

                _client.GetService<CommandService>().CreateCommand("serverIds")
                    .Alias("serverIds")
                    .Description("Exports roles IDs.\rPermission: Owner")
                    .MinPermissions((int)PermissionLevel.GroupAdmin)
                    .Do(async e =>
                    {
                        try
                        {
                            //extract the roles and id's from the server
                            List<Role> serverRolesList = e.Server.Roles.ToList();

                            string result = null;
                            for (int i = 0; i < serverRolesList.Count; i++)
                            {
                                result = result + $"{serverRolesList[i].Name} = {serverRolesList[i].Id}\r";
                            }

                            Channel userPM = await e.User.CreatePMChannel();
                            await userPM.SendMessage($"```\r{result}\r```");
                            _logs.logMessage("Info", "commandsSystem.serverIds", "User requested the server role IDs.", e.User.Name);
                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.serverIds", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("setGroup")
                    .Alias("sg")
                    .Description($"Sets the role group needed for basic commands.\r{_config.Prefix}setGroup default id\r{_config.Prefix}setGroup mods id\r{_config.Prefix}setGroup admins id\rPermission: Owner")
                    .Parameter("role", ParameterType.Optional)
                    .Parameter("id", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            ulong id = new ulong { };
                            try
                            {
                                id = Convert.ToUInt64(e.GetArg("id"));
                            }
                            catch
                            {

                            }
                            
                            if(e.GetArg("id") == null)
                            {
                                await e.Channel.SendMessage($"Please submit a role ID.\rExample: {_config.Prefix}setGroup default 139135090637668350");
                                return;
                            }

                            switch (e.GetArg("role"))
                            {
                                case "default":                                    
                                    _config.idDefaultGroup = id;
                                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                                    await e.Channel.SendMessage($"Permission Role has been updated.");
                                    _logs.logMessage("Info", "commandsSystem.setGroup default", $"Default role updated to {e.GetArg("id")}", e.User.Name);

                                    break;
                                case "mods":
                                    _config.idModsGroup = id;
                                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                                    await e.Channel.SendMessage($"Permission Role has been updated.");
                                    _logs.logMessage("Info", "commandsSystem.setGroup mods", $"Mods role updated to {e.GetArg("id")}", e.User.Name);
                                    break;
                                case "admins":
                                    _config.idAdminGroup = id;
                                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                                    await e.Channel.SendMessage($"Permission Role has been updated.");
                                    _logs.logMessage("Info", "commandsSystem.setGroup admins", $"Admins role updated to {e.GetArg("id")}", e.User.Name);
                                    break;
                                default:

                                    await e.Channel.SendMessage($"Please submit a group level.\rAvailable args\rdefault\rmods\radmins\rExample: {_config.Prefix}setGroup default 139135090637668350");
                                    break;
                            }

                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.setGroup", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("setPrefix")
                    .Alias("sp")
                    .Description("Changes the prefix that the bot will listen to.\rPermission: Owner")
                    .Parameter("id", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            if(e.GetArg("id") == "#" ||
                            e.GetArg("id") == "/" ||
                            e.GetArg("id") == "@")
                            {
                                await e.Channel.SendMessage($"Please pick another command character that is not one of the following.\r'#' '/' '@'");
                                return;
                            }
                            else
                            {
                                _config.Prefix = e.GetArg("id")[0];
                                _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                                await e.Channel.SendMessage($"Character prefix has been changed to {e.GetArg("id")} and will be active on next restart.");
                                _logs.logMessage("Info", "commandsSystem.setPrefix", $"Commands prefix was changed to {e.GetArg("id")}.", e.User.Name);
                            }
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.setPrefix", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("about")
                    .Alias("about")
                    .Description("Returns with github infomation.\rPermission: Everyone")
                    .Parameter("id", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            await e.Channel.SendMessage($"Here is my current documentation.\rGetting Started: soon\rCommands: <https://github.com/luther38/DiscordMusicBot/wiki/Commands>");
                            _logs.logMessage("Info", "commandsSystem.about", "User requested help docs.", e.User.Name);
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.about", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("export")
                    .Alias("export")
                    .Description($"Exports current files based on given arg.\r{_config.Prefix}export playlist\r{_config.Prefix}export blacklist\r{_config.Prefix}export log = Running log file.\rPermission: Mods")
                    .Parameter("file")
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        try
                        {
                            string argFile = e.GetArg("file");

                            Channel userPM = await e.User.CreatePMChannel();

                            switch (argFile)
                            {
                                case "playlist":
                                case "pl":
                                    bool plExport = _system.cmd_plExport();

                                    if (plExport == true)
                                    {
                                        await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");
                                        await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                        File.Delete(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");

                                        _logs.logMessage("Info", "commandsSystem.Export Playlist", "User requested the playlist file.", e.User.Name);
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                                    }

                                    break;
                                case "blacklist":
                                case "bl":
                                    bool blExport = _system.cmd_blExport();

                                    if (blExport == true)
                                    {                                        
                                        await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                                        await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                        File.Delete(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
 
                                        _logs.logMessage("Info", "commandsSystem.Export Blacklist", "User requested the blacklist file.", e.User.Name);
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                                    }

                                    break;
                                case "log":

                                    await userPM.SendFile(Directory.GetCurrentDirectory() + "\\logs.txt");
                                    await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                    _logs.logMessage("Info", "commandsSystem.Export log", "User requested the log file.", e.User.Name);

                                    break;
                                default:
                                    await e.Channel.SendMessage($"Invalid arguemnt found!\rPlease use one of the following.\r{_config.Prefix}export pl = Playlist\r{_config.Prefix}export bl = Blacklist\r{_config.Prefix}export log = Running log file.");
                                    break;

                                case "config":

                                    await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                                    await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                    _logs.logMessage("Info", "commandsSystem.Export config", "User requested the config file.", e.User.Name);
                                    break;
                            }

                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.export", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("ping")
                    .Alias("ping")
                    .Description($"Exports current files based on given arg.\r{_config.Prefix}export playlist\r{_config.Prefix}export blacklist\r{_config.Prefix}export log = Running log file.\rPermission: Mods")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            var t = e.Server.Region;

                            long ping = _network.cmd_ping(t.Hostname);
                            if (ping != -1)
                            {
                                await e.Channel.SendMessage($"{e.User.Name},\rDatacenter: {t.Name}\rPing: {ping}ms");
                                _logs.logMessage("Info", "commandSystem.ping", $"Datacenter: {t.Name} - Host IP: {t.Id} - Ping: {ping}ms", e.User.Name);
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{e.User.Name}, Something happened and I was unable to ping the server. Please check the log for the dump info.");
                            }
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.ping", error.ToString(), e.User.Name);
                        }
                    });

            });
        }
    }
}
