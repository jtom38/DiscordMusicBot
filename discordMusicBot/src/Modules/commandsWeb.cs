using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.Web;
using discordMusicBot.src.sys;
using Discord;
using Discord.Commands;


namespace discordMusicBot.src.Modules
{
    public class commandsWeb : ModuleBase<SocketCommandContext>
    {

        private CommandService _service;
        private configuration _config;

        urban _urban = new urban();
        rule34 _rule34 = new rule34();
        booru _danbooru = new booru();
        logs _logs = new logs();
        embed _embed = new embed();

        public commandsWeb(CommandService service)
        {
            _service = service;
        }

        [Command("UrbanDictionary"), Summary("Query's UrbanDictionary.com for a random definition."), Alias("ud")]
        public async Task UrbanDictionaryAsync(string tag = null)
        {
            try
            {
                string[] result = null;

                result = await _urban.cmd_urbanFlow(tag);
                
                if(result[0] == "No Value")
                {
                    var builder = await _embed.ErrorEmbedAsync("UrbanDictionary", $"**Term**: '{result[1]}'\r**Error**: Unable to find any infomation on this tag.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    var builder = await _embed.SucessEmbedAsync("UrbanDictionary", $"**Term**:\t'{result[2]}'\r**Definition**:\t{result[0]}\r**Example**:\t{result[1]}\r**Tags**:\t{result[3]}", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }

            }
            catch
            {

            }

        }

        [Command("smut"), Summary("Query's sites for smut related pictures."), Remarks("Query's sites for smut related pictures.")]
        public async Task smutAsync(string site = null, string tag = null)
        {
            try
            {
                //check to see if we need to spend smut to a spific room
                if(configuration.LoadFile().smutTextChannel != 0)
                {
                    //we need to check to make sure the room is correct
                }
                string[] result = null;

                switch (site)
                {
                    case "dabooru":
                    case "dan":
                    case "d":
                        {
                            result = await _danbooru.webRequestStart("danbooru", tag);
                        }
                        break;
                    case "konachan":
                    case "kona":
                    case "k":
                        {
                            result = await _danbooru.webRequestStart("konachan", tag);
                        }
                        break;
                    case "yandere":
                    case "yan":
                    case "y":
                        {
                            result = await _danbooru.webRequestStart("yandere", tag);
                        }
                        break;
                    case "rule34":
                    case "r34":
                    case "r":
                        {
                            result = await _rule34.rule34QuerrySite(tag);
                        }
                        break;
                    default:
                        result = await _rule34.rule34QuerrySite(site);
                        break;
                }

                if (result != null)
                {
                    var builder = await _embed.SucessEmbedAsync("Smut", $"{result[1]} with tag: {result[3]}", Context.User.Username, result[0]);
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    var builder = await _embed.SucessEmbedAsync("Smut", $"**Site**: '{result[1]}'\r**Error**: Unable to find any infomation on this tag.", Context.User.Username);
                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception error)
            {
                var builder = await _embed.ErrorEmbedAsync("Smut", error.ToString());
                await ReplyAsync("", false, builder.Build());

                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Smut", error.ToString(), Context.User.Username);
            }
        }

    }
}
