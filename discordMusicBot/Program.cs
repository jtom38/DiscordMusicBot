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
        startup _startup = new startup();

        public void loopRestart()
        {
            while(restartFlag == false)
            {
                Start();
            }
        }

        public void Start()
        {

            _startup.startupCheck();

            _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

            _client = new DiscordClient(x =>
            {
                x.AppName = "C# Music Bot";
                x.AppUrl = "https://github.com/luther38/DiscordMusicBot";
                x.AppVersion = "0.1.4";
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
            _client.AddModule<commandsWeb>("commandsWeb", ModuleFilter.ServerWhitelist);
            _client.GetService<AudioService>();

            //check the playlist file
            _playlist.shuffleLibrary();

            //this is used to force the bot the dc from the room if she is left alone.
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

                        //double checking to make sure she isnt in a room.  
                        //Event shouldnt have flagged but reguardless double checking
                        if (bot.ToString() != null) 
                        {
                            await bot.LeaveAudio();
                        }

                        //Console.WriteLine("Bot is left alone.  Music is stopping.");
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

