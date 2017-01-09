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
        discordStatus _discordStatus = new discordStatus();

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            _config = configuration.LoadFile();

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
                            _config.SaveFile();

                            await e.Channel.SendMessage("I have updated the config file for you.");
                            _logs.logMessage("Info", "commandsSystem.defaultRoom", $"defaultRoom was updated to {e.GetArg("roomID")}", e.User.Name);

                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.defaultRoom", error.ToString(), e.User.Name);
                        }

                    });

                _client.GetService<CommandService>().CreateCommand("volume")
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
                                await e.Channel.SendMessage($"{e.User.Name},\rPlease give me the percent value you want me to change to.\rExample: {_config.Prefix}vol 50");
                                return;
                            }

                            //make sure we got a number value
                            int intVol = 0;
                            int.TryParse(e.GetArg("vol"), out intVol);

                            if(intVol >= 1 && intVol <= 100)
                            {
                                //convert the value that we deam a percent value to a string to format it
                                string stringVol = intVol.ToString();
                                string t = null;
                                if(stringVol.Length == 1)
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
                                    _config.volume = newVol;
                                    _config.SaveFile();

                                    player.volume = newVol; //send the updated value to the var so we dont have to load the config file everytime in the loop.

                                    await e.Channel.SendMessage($"{e.User.Name},\rI have updated the volume to {e.GetArg("vol")}%.");
                                    _logs.logMessage("Info", "commandsSyste.Volume", $"Volume was changed to {newVol}%", e.User.Name);
                                }
                            }
                            else if(intVol >= 101)
                            {
                                await e.Channel.SendMessage($"{e.User.Name},\rThe value you gave was higher then 100%, sorry.");
                            }
                            else if(intVol <= 0)
                            {
                                await e.Channel.SendMessage($"{e.User.Name},\rThe value can't go below 1, sorry.");
                            }
                            
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
                    }) ;

                _client.GetService<CommandService>().CreateCommand("ping")
                    .Alias("ping")
                    .Description($"Exports current files based on given arg.\r{_config.Prefix}export playlist\r{_config.Prefix}export blacklist\r{_config.Prefix}export log = Running log file.\rPermission: Mods")
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            var t = e.Server.Region;

                            long ping = await _network.cmd_ping(t.Hostname);
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

                _client.GetService<CommandService>().CreateCommand("status")
                    .Alias("status")
                    .Description($"Returns the current tracked stats.\rPermission: Everyone")
                    .Parameter("value", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            _config = configuration.LoadFile(); //refresh the data

                            switch (e.GetArg("value").ToLower())
                            {
                                case "discord":
                                case "d":
                                    string[] status = await _discordStatus.getCurrentStatus();

                                    
                                    
                                    
                                    //await e.Channel.
                                    await e.Channel.SendMessage($"{status[0]}\r{status[1]}\r{status[2]}\r{status[3]}");
                                    break;

                                case "ping":
                                case "p":
                                    var t = e.Server.Region;
                                    long ping = await _network.cmd_ping(t.Hostname);

                                    await e.Channel.SendMessage($"Data Center: {t.Name}\r\tPing: {ping}");
                                    break;

                                case "system":
                                case "s":
                                    break;

                                case "volume":
                                case "v":

                                    await e.Channel.SendMessage($"Volume: {_config.volume}");
                                    break;

                                default:
                                    await e.Channel.SendMessage($"");
                                    break;
                            }


                            //await e.Channel.SendMessage($"rBot Uptime: Not Tracked Yet\rAudio Data\r\tTracks Played: Placeholder\r\t");
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.Status", error.ToString(), e.User.Name);
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("admin")
                    .Alias("a")
                    .Description($"Returns the current tracked stats.\rPermission: Everyone")
                    .Parameter("function", ParameterType.Optional)
                    .Parameter("value", ParameterType.Optional)
                    .Parameter("id",ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupAdmin)
                    .Do(async e =>
                    {
                        try
                        {
                            switch (e.GetArg("function").ToLower())
                            {
                                case "setsmut":
                                case "smut":
                                case "ss":
                                    try
                                    {

                                        if (e.GetArg("value") == "")
                                        {
                                            await e.Channel.SendMessage($"Please submit a Text Channel ID for smut to configure where smut can be sent to.\rExample: {_config.Prefix}admin setSmut 139135090637668350");
                                            return;
                                        }
                                        else
                                        {
                                            ulong id = new ulong { };
                                            try
                                            {
                                                id = Convert.ToUInt64(e.GetArg("value"));
                                            }
                                            catch
                                            {

                                            }

                                            _config.smutTextChannel = id;
                                            _config.SaveFile();

                                            await e.Channel.SendMessage($"Smut Text Channel has been updated.");
                                            _logs.logMessage("Info", "commandsSystem.admin.setSmut", $"Smut Text Channel updated to {e.GetArg("id")}", e.User.Name);
                                        }
                                        
                                    }
                                    catch(Exception error)
                                    {
                                        _logs.logMessage("Error", "commandsSystem.admin.setSmut", error.ToString(), "System");
                                    }
                                    break;

                                case "setprefix":
                                case "prefix":
                                case "sp":
                                    try
                                    {
                                        if (e.GetArg("value") == "#" ||
                                            e.GetArg("value") == "/" ||
                                            e.GetArg("value") == "@")
                                        {
                                            await e.Channel.SendMessage($"Please pick another command character that is not one of the following.\r'#' '/' '@'");
                                            return;
                                        }
                                        else if(e.GetArg("value") == "")
                                        {
                                            await e.Channel.SendMessage($"Please submit a value to make as a new command character.");
                                            return;
                                        }
                                        else
                                        {
                                            _config.Prefix = e.GetArg("value")[0];
                                            _config.SaveFile();

                                            await e.Channel.SendMessage($"Character prefix has been changed to {e.GetArg("value")} and will be active on next restart.");
                                            _logs.logMessage("Info", "commandsSystem.admin.setPrefix", $"Commands prefix was changed to {e.GetArg("value")}.", e.User.Name);
                                        }
                                    }
                                    catch (Exception error)
                                    {
                                        _logs.logMessage("Error", "commandsSystem.admin.setPrefix", error.ToString(), e.User.Name);
                                    }
                                    break;

                                case "export":
                                case "e":
                                    try
                                    {
                                        Channel userPM = await e.User.CreatePMChannel();

                                        switch (e.GetArg("value"))
                                        {
                                            case "playlist":
                                            case "pl":
                                                bool plExport = await _system.cmd_plExport();

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
                                                bool blExport = await _system.cmd_blExport();

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
                                            case "config":
                                                await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                                                await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                                _logs.logMessage("Info", "commandsSystem.Export config", "User requested the config file.", e.User.Name);

                                                break;
                                            default:
                                                await e.Channel.SendMessage($"Invalid arguemnt found!\rPlease use one of the following.\r{_config.Prefix}export pl = Playlist\r{_config.Prefix}export bl = Blacklist\r{_config.Prefix}export log = Running log file.");
                                                break;
                                        }

                                    }
                                    catch (Exception error)
                                    {
                                        _logs.logMessage("Error", "commandsSystem.export", error.ToString(), e.User.Name);
                                    }
                                    break;

                                case "setgroup":
                                case "group":
                                case "sg":
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

                                        if (e.GetArg("id") == "")
                                        {
                                            await e.Channel.SendMessage($"Please submit a role ID.\rExample: {_config.Prefix}setGroup default 139135090637668350");
                                            return;
                                        }

                                        switch (e.GetArg("value"))
                                        {
                                            case "default":
                                                _config.idDefaultGroup = id;
                                                _config.SaveFile();

                                                await e.Channel.SendMessage($"Permission Role has been updated.");
                                                _logs.logMessage("Info", "commandsSystem.admin.setGroup default", $"Default role updated to {e.GetArg("id")}", e.User.Name);

                                                break;
                                            case "mods":
                                                _config.idModsGroup = id;
                                                _config.SaveFile();

                                                await e.Channel.SendMessage($"Permission Role has been updated.");
                                                _logs.logMessage("Info", "commandsSystem.admin.setGroup mods", $"Mods role updated to {e.GetArg("id")}", e.User.Name);
                                                break;
                                            case "admins":
                                                _config.idAdminGroup = id;
                                                _config.SaveFile();

                                                await e.Channel.SendMessage($"Permission Role has been updated.");
                                                _logs.logMessage("Info", "commandsSystem.admin.setGroup admins", $"Admins role updated to {e.GetArg("id")}", e.User.Name);
                                                break;
                                            default:

                                                await e.Channel.SendMessage($"Please submit a group level.\rAvailable args\rdefault\rmods\radmins\rExample: {_config.Prefix}admin setGroup default 139135090637668350");
                                                break;
                                        }

                                    }
                                    catch (Exception error)
                                    {
                                        _logs.logMessage("Error", "commandsSystem.admin.setGroup", error.ToString(), e.User.Name);
                                    }
                                    break;

                                case "maxSubmitted":
                                case "submit":
                                case "ms":
                                    try
                                    {
                                        if (e.GetArg("value") == "")
                                        {
                                            await e.Channel.SendMessage($"{e.User.Name},\rMax Submitted: {_config.maxTrackSubmitted}\rIf you want to disable this enter value '-1'");
                                        }
                                        else
                                        {
                                            int value = -2;
                                            bool parseResult = int.TryParse(e.GetArg("value"), out value);

                                            if (parseResult == true)
                                            {
                                                if (value == -1)
                                                {
                                                    _config.maxTrackSubmitted = value;
                                                    _config.SaveFile();
                                                    await e.Channel.SendMessage($"{e.User.Name},\rI have disabled this function for you.");
                                                    _logs.logMessage("Info", "commandsSystem.admin.maxSubmitted", $"Max number of submitted tracks is now disabled", e.User.Name);
                                                }
                                                else if (value != -2)
                                                {
                                                    await e.Channel.SendMessage($"Error generated.  Default value was found.");
                                                }
                                                else
                                                {
                                                    _config.maxTrackSubmitted = value;
                                                    _config.SaveFile();
                                                    await e.Channel.SendMessage($"{e.User.Name},\rI have adjusted the max number of submitted tracks to {e.GetArg("value")}.");
                                                    _logs.logMessage("Info", "commandsSystem.admin.maxSubmitted", $"Max number of submitted tracks is now {e.GetArg("value")}", e.User.Name);
                                                }
                                            }
                                            else
                                            {
                                                await e.Channel.SendMessage($"{e.User.Name},\rPlease enter a number value to adjust the max number of tracks a user can submit.");
                                            }
                                        }

                                    }
                                    catch (Exception error)
                                    {
                                        _logs.logMessage("Error", "commandsSystem.admin.maxSubmitted", error.ToString(), e.User.Name);
                                    }
                                    break;

                                case "serverids":
                                case "ids":
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

                                        _logs.logMessage("Info", "commandsSystem.admin.serverIds", "User requested the server role IDs.", e.User.Name);
                                    }
                                    catch (Exception error)
                                    {
                                        _logs.logMessage("Error", "commandsSystem.admin.serverIds", error.ToString(), e.User.Name);
                                    }

                                    break;
                                default:
                                    await e.Channel.SendMessage($"Available Commands for !admin\r!admin setSmut\r!admin setPrefix\r!admin setGroup\r!admin export\r!admin maxSubmitted\r!admin serverIDs");
                                    break;
                            }
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsSystem.maxSubmitted", error.ToString(), e.User.Name);
                        }
                    });
            });
        }
    }
}
