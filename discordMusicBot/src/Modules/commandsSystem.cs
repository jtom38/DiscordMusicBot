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

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

            manager.CreateCommands("", group =>
            {
                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "rm")
                    .Alias("rm")
                    .Description("Removes messages from a text channel.\rExample: !rm 100\rPermissions: Everyone")
                    .Parameter("count", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {

                        if (e.GetArg("count") == "")
                        {
                            await e.Channel.SendMessage($"@ Cant delete if you dont tell me how many to remove..");
                            return;
                        }

                        //make var to store messages from the server
                        Message[] messagesToDelete;

                        //convert arg to int
                        int count = int.Parse(e.GetArg("count"));

                        //tell server to download messages to memory
                        messagesToDelete = await e.Channel.DownloadMessages(count);

                        //tell bot to delete them from server
                        await e.Channel.DeleteMessages(messagesToDelete);

                        //await e.Channel.SendMessage($"@{e.User.Name}, I have added {e.GetArg("url")} to autoplaylist.txt.");
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "plexport")
                    .Alias(new string[] { "plexport", "ple" })
                    .Description("Exports current playlist \rPermission: Mods")
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        bool result = _playlist.cmd_plexport();
                        if(result == true)
                        {
                            Channel userPM = await e.User.CreatePMChannel();

                            await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");
                            await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                            File.Delete(Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json");
                        }
                        else
                        {
                            await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                        }                    
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "blexport")
                    .Alias(new string[] { "blexport", "ble" })
                    .Description("Exports current blacklist\rPermission: Mods")
                    .MinPermissions((int)PermissionLevel.GroupMods)
                    .Do(async e =>
                    {
                        bool result = _playlist.cmd_blexport();

                        if(result == true)
                        {
                            Channel userPM = await e.User.CreatePMChannel();
                            await userPM.SendFile(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                            await e.Channel.SendMessage($"{e.User.Name},\rPlease check the PM that I sent you for your file request.");
                            File.Delete(Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json");
                        }
                        else
                        {
                            await e.Channel.SendMessage("Error generating file.\rPlease inform the server owner for more infomation.");
                        }
                    });

                _client.GetService<CommandService>().CreateCommand(_config.Prefix + "defaultRoom")
                    .Alias("defaultRoom")
                    .Description("Sets the bots default voice room.\rExample: !defaultRoom roomID\rPermission: Owner")
                    .Parameter("roomID", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        if(e.GetArg("roomID") == null)
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
                    });

                _client.GetService<CommandService>().CreateCommand("vol")
                    .Alias("vol")
                    .Description("Adjusts the default volume from the bot.\rExample: !vol +10\rPermission: Everyone")
                    .Parameter("vol", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
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
                            if(argOperator != "+" || argOperator != "-")
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

                        if(argOperator == "-")
                        {
                            newValue = oldVolume - argValue;
                        }

                        _config.volume = newValue;
                        _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                        await e.Channel.SendMessage($"{e.User.Name} changed default volume from "+ oldVolume + " to " +newValue + ".");
                        Console.WriteLine("{e.User.Name} changed default volume from " + oldVolume + " to " + newValue + ".");
                    });

                _client.GetService<CommandService>().CreateCommand("halt")
                    .Alias("halt")
                    .Description("Shuts down the bot.\rPermission: Owner")
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {

                        await e.Channel.SendMessage($":wave: :zzz:");

                        await e.Server.Client.Disconnect();

                        Environment.Exit(0);
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

                _client.GetService<CommandService>().CreateCommand("setGroupDefault")
                    .Alias("sgd")
                    .Description("Sets the role group needed for basic commands.\rPermission: Owner")
                    .Parameter("id", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            ulong id = Convert.ToUInt64(e.GetArg("id"));
                            _config.idDefaultGroup = id;
                            _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                            await e.Channel.SendMessage($"Permission Role has been updated.");
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine($"Error: setGroupDefault generated a error: {error}");
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("setGroupMods")
                    .Alias("sgm")
                    .Description("Sets the role group needed for mod commands.\rPermission: Owner")
                    .Parameter("id", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            ulong id = Convert.ToUInt64(e.GetArg("id"));
                            _config.idModsGroup = id;
                            _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                            await e.Channel.SendMessage($"Permission Role has been updated.");
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"Error: setGroupDefault generated a error: {error}");
                        }
                    });

                _client.GetService<CommandService>().CreateCommand("setGroupAdmins")
                    .Alias("sga")
                    .Description("Sets the role group needed for admin commands.\rPermission: Owner")
                    .Parameter("id", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.BotOwner)
                    .Do(async e =>
                    {
                        try
                        {
                            ulong id = Convert.ToUInt64(e.GetArg("id"));
                            _config.idAdminGroup = id;
                            _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                            await e.Channel.SendMessage($"Permission Role has been updated.");
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"Error: setGroupDefault generated a error: {error}");
                        }
                    });

            });
        }
    }
}
