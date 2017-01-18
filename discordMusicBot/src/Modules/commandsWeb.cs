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
                    var builder = new EmbedBuilder()
                    {
                        //unit error = uint.Parse(colors.Error)
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Description = $"**Term**: '{result[1]}'\r**Error**: Unable to find any infomation on this tag."
                    };

                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                        Description = $"**Term**:\t'{result[2]}'\r**Definition**:\t{result[0]}\r**Example**:\t{result[1]}\r**Tags**:\t{result[3]}"
                    };

                    await ReplyAsync("", false, builder.Build());
                }

            }
            catch
            {

            }

        }

        [Command("smut"), Summary("Query's sites for smut related pictures.")]
        public async Task smutAsync(string site = null, string tag = null)
        {
            try
            {
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
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(colors.Success[0], colors.Success[1], colors.Success[2]),
                        Title = $"{result[1]} with tag: {result[3]}",
                        ImageUrl = result[0]
                    };

                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(colors.Error[0], colors.Error[1], colors.Error[2]),
                        Description = $"**Site**: '{result[1]}'\r**Error**: Unable to find any infomation on this tag."
                    };

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch
            {

            }
        }

    }
}
