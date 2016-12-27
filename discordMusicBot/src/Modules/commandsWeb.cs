using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.Web;
using discordMusicBot.src.sys;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;

namespace discordMusicBot.src.Modules
{
    internal class commandsWeb : IModule
    {

        private ModuleManager _manager;
        private DiscordClient _client;
        private configuration _config;

        urban _urban = new urban();
        rule34 _rule34 = new rule34();
        booru _danbooru = new booru();
        logs _logs = new logs();

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", group =>
            {
                _client.GetService<CommandService>().CreateCommand("urbanDictionary")
                    .Alias("ud")
                    .Description("Requests a definition from UrbanDictionary.com.\rExample: !urbanDictionary 'cheesy ragu'\rPermissions: Everyone")
                    .Parameter("tag", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            string[] result = null;

                            if(e.GetArg("tag") == "")
                            {
                                //request a random tag from what we have so far in the urban.json file.. to be built
                                result = _urban.cmd_urbanFlow(null);
                            }
                            else
                            {
                                result = _urban.cmd_urbanFlow(e.GetArg("tag"));
                            }



                            if(result == null)
                            {
                                await e.Channel.SendMessage($"Ran into a error trying to process your request.  The error has been saved to the log file.");
                            }
                            else if(result[0] == "No Value")
                            {
                                await e.Channel.SendMessage($"{e.User.Name},\rI searched for '{e.GetArg("tag")}' but nothing was found.");
                            }
                            else
                            {
                                string message = $"```\rUrbanDictionary Restult for '{result[2]}'\rDefinition: {result[0]}\rExample: {result[1]}\rTags: {result[3]}```";

                                //check to see if the message is longer then 2000 characters.  Discord Limmit.
                                int messageLength = message.Length;

                                if(message.Length >= 2000)
                                {
                                    //get the number of characters we need to remove

                                    int charTrim = message.Length - 2003;
                                    message.Substring(message.Length - charTrim);
                                }

                                await e.Channel.SendMessage(message);
                            }


                        }
                        catch(Exception error)
                        {
                            _logs.logMessage("Error", "commandsWeb.urbanDictionary", error.ToString(), e.User.Name);
                        }


                    });

                _client.GetService<CommandService>().CreateCommand("smut")
                    .Alias("smut")
                    .Description("Requests a picture from r34.xxx.\rExample: !smut 'overwatch'\rPermissions: Everyone")
                    .Parameter("site", ParameterType.Optional)
                    .Parameter("tag", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.GroupUsers)
                    .Do(async e =>
                    {
                        try
                        {
                            string[] result = null;

                            switch (e.GetArg("site"))
                            {
                                case "dabooru":
                                case "dan":
                                case "d":
                                    result = _danbooru.webRequestStart("danbooru", e.GetArg("tag"));

                                    if(result != null)
                                    {
                                        //await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}\rTags: {result[2]}");
                                        await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage($"Unable to find anything with the tag: {e.GetArg("tag")}");
                                    }
                                    break;
                                case "konachan":
                                case "kona":
                                case "k":
                                    {
                                        result = _danbooru.webRequestStart("konachan", e.GetArg("tag"));

                                        if (result != null)
                                        {
                                            await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}\rTags: {result[2]}");
                                            //await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessage($"Unable to find anything with the tag: {e.GetArg("tag")}");
                                        }
                                    }
                                    break;
                                case "yandere":
                                case "yan":
                                case "y":
                                    {
                                        result = _danbooru.webRequestStart("yandere", e.GetArg("tag"));

                                        if (result != null)
                                        {
                                            await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}\rTags: {result[2]}");
                                            //await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessage($"Unable to find anything with the tag: {e.GetArg("tag")}");
                                        }
                                    }
                                    break;
                                case "rule34":
                                case "r34":
                                case "r":
                                    result = _rule34.rule34QuerrySite(e.GetArg("tag"));

                                    if( result != null)
                                    {
                                        await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}\rTags: {result[2]}");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage($"Unable to find anything with the tag: {e.GetArg("tag")} on 'rule34.xxx'.");
                                    }
                                    break;
                                default:
                                    Random rng = new Random();
                                    int num = rng.Next(0, 2); //max is the number of sites

                                    switch (num)
                                    {
                                        case 0:
                                            result = _danbooru.webRequestStart("danbooru", e.GetArg("site"));
                                            break;
                                        case 1:
                                            result = _danbooru.webRequestStart("konachan", e.GetArg("tag"));
                                            break;
                                        case 2:
                                            result = _danbooru.webRequestStart("yandere", e.GetArg("tag"));
                                            break;
                                        case 3:
                                            break;
                                    }
                                    
                                    if (result != null)
                                    {
                                        await e.Channel.SendMessage($"Result from {result[1]}\rURL: {result[0]}\rTags: {result[2]}");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage($"Unable to find anything with the tag: {e.GetArg("site")}");
                                    }
                                    break;
                            }

                            //await e.Channel.SendMessage("placeholder");
                        }
                        catch (Exception error)
                        {
                            _logs.logMessage("Error", "commandsWeb.urbanDictionary", error.ToString(), e.User.Name);
                        }


                    });
            });
        }
    }
}
