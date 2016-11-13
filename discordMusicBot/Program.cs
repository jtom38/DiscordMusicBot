using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Modules;
using discordMusicBot.src;
using discordMusicBot.src.Modules;

namespace discordMusicBot
{
    public class Program
    {

        static void Main(string[] args) => new Program().Start();

        private DiscordClient _client;
        private configuration _config;

        playlist _playlist = new playlist();

        const string configFile = "config.json";

        public void Start()
        {

            startupCheck();

            _config = configuration.LoadFile(configFile);

            _client = new DiscordClient(x =>
            {
                x.AppName = "C# Music Bot";
                x.AppUrl = "https://github.com/luther38/DiscordMusicBot";
                x.LogLevel = LogSeverity.Info;
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
            });
            
            //this CommandsModule is tied behind discordMusicBot.src
            _client.AddModule<commandsPlayer>("commandsPlayer", ModuleFilter.ServerWhitelist);
            _client.AddModule<commandsSystem>("commandsSystem", ModuleFilter.ServerWhitelist);
            _client.AddModule<commandsPlaylist>("commandsPlaylist", ModuleFilter.ServerWhitelist);
            _client.GetService<AudioService>();

            //check the playlist file
            _playlist.getPlaylistFile();

            //turns the bot on and connects to discord.
            _client.ExecuteAndWait(async () =>
            {
                while (true)
                {
                    try
                    {
                        await _client.Connect(_config.Token, TokenType.Bot);
                        _client.SetGame("Discord.Net");
                        Console.WriteLine("Connected to Discord.");
                        //await _client.ClientAPI.Send(new Discord.API.Client.Rest.HealthRequest());
                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Login Failed", ex);
                        await Task.Delay(_client.Config.FailedReconnectDelay);
                    }
                }
            });
        }

        private void startupCheck()
        {
            makeCacheFolder();
            checkConfigFile();
            checkToken();
            checkPlaylistURL();
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

        private void checkConfigFile()
        {
            try
            {
                _config = configuration.LoadFile(configFile);

            }
            catch
            {
                //unable to find the file
                _config = new configuration();
                _config.SaveFile(configFile);
            }
        }

        private void checkToken()
        {
            //check for the bot token
            try
            {
                _config = configuration.LoadFile(configFile);
                if(_config.Token != "")
                {
                    Console.WriteLine("Token has been found in config.json");
                }
                else
                {
                    Console.WriteLine("Please enter a valid token.");
                    Console.Write("Token: ");

                    _config.Token = Console.ReadLine();                     // Read the user's token from the console.
                    _config.SaveFile(configFile);
                }          
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        //might be dropped
        private void checkPlaylistURL()
        {
            try
            {
                _config = configuration.LoadFile(configFile);
                if (_config.PlaylistURL != "")
                {
                    Console.WriteLine("PlaylistURL has been found in config.json");
                }
                else
                {
                    Console.WriteLine("Please enter a valid url for a txt.  \r If you want to disable this type 'disable'.");
                    Console.Write("PlaylistURL: ");

                    _config.PlaylistURL = Console.ReadLine();                     // Read the user's token from the console.
                    _config.SaveFile(configFile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
        
        private void checkCommandPrefix()
        {
            Console.WriteLine("Current commandPrefix = " + _config.Prefix);
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

    }
}

