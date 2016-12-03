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

namespace discordMusicBot.src.Modules
{
    internal class commandsSystem : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private configuration _config;

        playlist _playlist = new playlist();
        system _system = new system();

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

                            while(counter != 0)
                            {
                                if(counter >= 100)
                                {
                                    deleteCounter = 100;
                                }
                                else
                                {
                                    deleteCounter = counter;
                                }
                                    

                                //make var to store messages from the server
                                Message[] messagesToDelete;

                                //tell server to download messages to memory
                                messagesToDelete = await e.Channel.DownloadMessages(deleteCounter);

                                //tell bot to delete them from server
                                await e.Channel.DeleteMessages(messagesToDelete);

                                counter = counter - 100;
                            }

                            //await e.Channel.SendMessage($"@{e.User.Name}, I have added {e.GetArg("url")} to autoplaylist.txt.");
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine($"Error generated with !plExport\rDump: {error}");
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
                            Console.WriteLine("Config.json update: defaultRoomID = " + id);
                            //await e.Channel.SendFile("blacklist.json");
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine($"Error generated with !defaultRoom\rDump: {error}");
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
                            Console.WriteLine("{e.User.Name} changed default volume from " + oldVolume + " to " + newValue + ".");
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine($"Error Generated with !vol\rDump: {error}");
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

                            await e.Server.Client.Disconnect();

                            Environment.Exit(0);
                        }
                        catch
                        {

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
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine($"Error with !serverIds: Error dump: {error}");
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

                                    break;
                                case "mods":
                                    _config.idModsGroup = id;
                                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                                    await e.Channel.SendMessage($"Permission Role has been updated.");
                                    break;
                                case "admins":
                                    _config.idAdminGroup = id;
                                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                                    await e.Channel.SendMessage($"Permission Role has been updated.");
                                    break;
                                default:

                                    await e.Channel.SendMessage($"Please submit a group level.\rAvailable args\rdefault\rmods\radmins\rExample: {_config.Prefix}setGroup default 139135090637668350");
                                    break;
                            }

                        }
                        catch(Exception error)
                        {
                            Console.WriteLine($"Error: setGroupDefault generated a error: {error}");
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
                            }
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"Error generated with !setPrefix\rDump: {error}");
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
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"Error generated with !setPrefix\rDump: {error}");
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
                                    bool plExport = _system.cmd_plExport();

                                    if (plExport == true)
                                    {
                                        await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");
                                        await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                        File.Delete(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                                    }

                                    break;
                                case "blacklist":
                                    bool blExport = _system.cmd_blExport();

                                    if (blExport == true)
                                    {                                        
                                        await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                                        await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                        File.Delete(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                                    }

                                    break;
                                case "log":
                                    bool logExport = _system.cmd_exportLog();

                                    if (logExport == true)
                                    {
                                        //await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                                        //await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                                        //File.Delete(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                                    }

                                    break;
                                default:
                                    await e.Channel.SendMessage($"Invalid arguemnt found!\rPlease use one of the following.\r{_config.Prefix}export pl = Playlist\r{_config.Prefix}export bl = Blacklist\r{_config.Prefix}export log = Running log file.");
                                    break;
                            }

                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"Error generated with !exportLog\rDump: {error}");
                        }
                    });

            });
        }
    }
}
