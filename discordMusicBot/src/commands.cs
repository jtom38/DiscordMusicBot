using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;

namespace discordMusicBot.src.Commands
{
    internal class CommandsModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", group =>
            {
                //group.PublicOnly();

                _client.GetService<CommandService>().CreateCommand("about")
                    .Alias("about")
                    .Description("test")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage($"Hi, {e.User.Name} my name is Music-Bot-Test aka Momo. :smile:");
                    });

                _client.GetService<CommandService>().CreateCommand("getpl")
                    .Alias("getpl")
                    .Description("Goes out and fetches our google doc playlist file and updates the local copy.")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage($"Please wait... fetching the file");
                        playlist _playlist = new playlist();
                        //await _playlist.updatePlaylistFile();
                    });

                _client.GetService<CommandService>().CreateCommand("greet") //create command greet
                    .Alias(new string[] { "gr", "hi" }) //add 2 aliases, so it can be run with ~gr and ~hi
                    .Description("Greets a person.") //add description, it will be shown when ~help is used
                    .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
                        //sends a message to channel with the given text
                    });


            });

        }
    }
}
