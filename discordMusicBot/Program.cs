using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Audio;

namespace discordMusicBot
{
    public class Program
    {

        static void Main(string[] args) => new Program().Start();

        private DiscordClient _client;

        public void Start()
        {
            _client = new DiscordClient(x =>
            {
                x.AppName = "C# Music Bot";
                x.AppUrl = "https://github.com/luther38/DiscordMusicBot";
            })
            .UsingCommands(x =>
            {
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
            })
            .UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                x.EnableEncryption = true;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            });

            //cmd_echo();

            _client.GetService<CommandService>().CreateCommand("greet") //create command greet
                .Alias(new string[] { "gr", "hi" }) //add 2 aliases, so it can be run with ~gr and ~hi
                .Description("Greets a person.") //add description, it will be shown when ~help is used
                .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
                    //sends a message to channel with the given text
                });


            //turns the bot on and connects to discord.
            _client.ExecuteAndWait(async () =>
            {
                //get the bot token from the config file
                CheckBotToken();

                while (true)
                {
                    try
                    {
                        await _client.Connect(config.Default.botToken, TokenType.Bot);
                        _client.SetGame("Discord.Net");
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
                //_client.ReplyError(e, msg);
                _client.Log.Error("Command", msg);
            }
        }

        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
        }

        private void CheckBotToken()
        {
            if (config.Default.botToken == "")
            {
                Console.WriteLine("Error: Unable to find Bot Token.  Please add the token to config.settings.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("botToken:" + config.Default.botToken);
            }          
        }

        private void cmd_echo()
        {
            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                    await e.Channel.SendMessage(e.Message.Text);
            };
        }
    }
}

