using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using discordMusicBot.src;
using discordMusicBot.src.Modules;

namespace discordMusicBot
{
    public enum PermissionLevel : byte
    {
        NoAccess = 0, //everyone
        GroupUsers,
        GroupMods, //Bot Mods
        GroupAdmin, //Bot Admins
        BotOwner, //Bot Owner (Global)
    }

    public class Program
    {
        static void Main(string[] args) => new Program().Start();

        public static bool restartFlag = false;

        private IAudioClient _voice;    //being used so the event can disconnect the bot from the room when she is left alone
        private DiscordClient _client;
        private configuration _config;

        player _player = new player();
        playlist _playlist = new playlist();
        logs _logs = new logs();

        public void loopRestart()
        {
            while(restartFlag == false)
            {
                Start();
            }
        }

        public void Start()
        {

            startupCheck();

            _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

            _client = new DiscordClient(x =>
            {
                x.AppName = "C# Music Bot";
                x.AppUrl = "https://github.com/luther38/DiscordMusicBot";
                x.AppVersion = "0.1.0";
                x.UsePermissionsCache = true;
                //x.LogLevel = LogSeverity.Info;
                x.LogHandler = OnLogMessage;
            })
            .UsingCommands(x =>
            {
                x.PrefixChar = _config.Prefix;
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
            })
            .UsingModules()
            .UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                x.EnableEncryption = true;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            })
            .UsingPermissionLevels(PermissionResolver);
            
            //this CommandsModule is tied behind discordMusicBot.src
            _client.AddModule<commandsPlayer>("commandsPlayer", ModuleFilter.ServerWhitelist);
            _client.AddModule<commandsSystem>("commandsSystem", ModuleFilter.ServerWhitelist);
            _client.AddModule<commandsPlaylist>("commandsPlaylist", ModuleFilter.ServerWhitelist);
            _client.GetService<AudioService>();

            //check the playlist file
            //_playlist.getPlaylistFile();
            //_playlist.loadPlaylist();
            //_playlist.loadBlacklist();

            _playlist.shuffleLibrary();

            _client.UserUpdated += async (s, e) =>
            {

                //gives us more infomation for like what room the bot is in
                var bot = e.Server.FindUsers(_client.CurrentUser.Name).FirstOrDefault().VoiceChannel;

                try
                {
                    List<User> userCount = bot.Users.ToList();

                    if (userCount.Count <= 1)
                    {
                        _client.SetGame(null);
                        _player.cmd_stop();
                        //Channel voiceChan = e.User.VoiceChannel;
                        
                        await bot.LeaveAudio();

                        //await _voice.Disconnect();
                        //Console.WriteLine("Bot is left alone.  Music is stopping.");
                    }
                    else
                    {
                        //pushing this resume to beta... just need more time and refactoring to get this working the way I want.
                        _player.cmd_resume();
                        //Console.WriteLine("Someone joined the room.  Starting next track.");
                    }
                }
                catch
                {
                    //this will catch if the bot isnt summoned given bot.user.tolist will pull a null
                }

            };

            //turns the bot on and connects to discord.
            _client.ExecuteAndWait(async () =>
            {
                while (true)
                {
                    try
                    {
                        await _client.Connect(_config.Token, TokenType.Bot);
                        _client.SetGame(null);
                        _logs.logMessage("Info", "program.Start", "Connected to Discord", "system");
                        //Console.WriteLine("Connected to Discord.");
                        //await _client.ClientAPI.Send(new Discord.API.Client.Rest.HealthRequest());

                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Login Failed", ex);
                        _logs.logMessage("Error", "program.Start", ex.ToString(), "system");
                        await Task.Delay(_client.Config.FailedReconnectDelay);
                    }
                }
            });
        }

        private void startupCheck()
        {
            makeCacheFolder();
            makeConfigFolder();
            checkConfigFile();
            checkToken();
            setOwnerID();
            setLogLevel();
            checkCommandPrefix();
        }

        private void makeCacheFolder()
        {
            if (Directory.Exists("cache"))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory("cache");
            }        
        }

        private void makeConfigFolder()
        {
            if (Directory.Exists("configs"))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory("configs");
            }
        }

        private void checkConfigFile()
        {
            var configPath = Directory.GetCurrentDirectory() + "\\configs\\config.json";

            try
            {
                if(File.Exists(configPath))
                {
                    _config = configuration.LoadFile(configPath);
                }
                else
                {
                    _config = new configuration();
                    _config.SaveFile(configPath);
                }
                

            }
            catch
            {
                //unable to find the file
                _config = new configuration();
                _config.SaveFile(configPath);
            }
        }

        private void checkToken()
        {
            //check for the bot token
            try
            {
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                if(_config.Token != "")
                {
                    Console.WriteLine("Token has been found in config.json");
                }
                else
                {
                    Console.WriteLine("Please enter a valid token.");
                    Console.Write("Token: ");

                    _config.Token = Console.ReadLine();                     // Read the user's token from the console.
                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                }          
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        private void setOwnerID()
        {
            try
            {
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                //ulong ownerID = _config.Owner;

                if (Int64.Parse(_config.Owner.ToString()) != 0)
                {
                    Console.WriteLine("Owner ID has been found in config.json");
                }
                else
                {
                    Console.WriteLine("Please enter your user ID to take ownership of this bot.");
                    Console.Write("ID: ");

                    ulong id = Convert.ToUInt64(Console.ReadLine());

                    _config.Owner = id;
                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                }
            }
            catch(Exception error)
            {
                Console.WriteLine($"Error: {error}");
            }
        }
        
        private void checkCommandPrefix()
        {
            Console.WriteLine("Current commandPrefix = " + _config.Prefix);
        }

        private void setLogLevel()
        {
            try
            {
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                int t = _config.logLevel;
                if (_config.logLevel >= 0)
                {
                    switch (_config.logLevel)
                    {
                        case 0: //off
                            Console.WriteLine($"Logging: Off");
                            break;
                        case 1: //debug
                            Console.WriteLine($"Logging: Debug");
                            break;
                        case 2: //info
                            Console.WriteLine($"Logging: Infomation");
                            break;
                        case 3: //error
                            Console.WriteLine($"LOgging: Errors");
                            break;
                    }
                    
                }
                else
                {
                    Console.WriteLine("Please enter what level of logging you would like.");
                    Console.WriteLine("0: Off");
                    Console.WriteLine("1: Debug");
                    Console.WriteLine("2: Infomation");
                    Console.WriteLine("3: Errors");
                    Console.Write("LogLevel: ");

                    int logLevel = 0;

                    int.TryParse(Console.ReadLine(), out logLevel);

                    _config.logLevel = logLevel;
                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error: {error}");
            }
        }

        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        msg = "Unknown command.";
                        break;
                }
            }
            if (msg != null)
            {
                //TODO not sure why ReployError came back missing something.
                //_client.ReplyError(e, msg);
                e.Channel.SendMessage(msg);
                _client.Log.Error("Command", msg);
            }
        }

        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        private int PermissionResolver(User user, Channel channel)
        {
            List<Role> list = user.Roles.ToList();

            if (user.Id == _config.Owner)
                return (int)PermissionLevel.BotOwner;

            if (user.Server != null)
            {
                //figure out Bot Admins
                if (user.Roles.Any(x => x.Id == _config.idAdminGroup))
                    return (int)PermissionLevel.GroupAdmin;

                //figure out Bot Mods
                if (user.Roles.Any(x => x.Id == _config.idModsGroup))
                    return (int)PermissionLevel.GroupMods;

                //figure out if they have permissions to the bot
                if (user.Roles.Any(x => x.Id == _config.idDefaultGroup))
                    return (int)PermissionLevel.GroupUsers;

            }
            return (int)PermissionLevel.NoAccess;
        }

    }
}

