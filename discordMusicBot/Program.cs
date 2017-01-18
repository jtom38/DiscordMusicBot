using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using discordMusicBot.src;
using discordMusicBot.src.sys;
using discordMusicBot.src.audio;
using discordMusicBot.src.Modules;
using Discord.WebSocket;

namespace discordMusicBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _commands;
        private configuration _config;
        private startup _startup;

        public async Task Start()
        {
            //_startup.startupCheck(); // chances are this will fail

            // Create a new instance of DiscordSocketClient.
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose                  // Specify console verbose information level.
            });

            _client.Log += (l)                               // Register the console log event.
                => Task.Run(()
                => Console.WriteLine($"[{l.Severity}] {l.Source}: {l.Exception?.ToString() ?? l.Message}"));

            //need to add event to check for users 

            await _client.LoginAsync(TokenType.Bot, configuration.LoadFile().Token);
            await _client.ConnectAsync();

            _commands = new CommandHandler();               // Initialize the command handler service
            await _commands.Install(_client);

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }
    }
}