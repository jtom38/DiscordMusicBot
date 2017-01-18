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

        [Command("UrbanDictionary"), 
            Summary("Query's UrbanDictionary.com for a random definition."),
            Alias("ud")]
        public async Task UrbanDictionaryAsync(string tag = null)
        {
            try
            {
                string[] result = null;
                if (tag == null)
                {
                    result = await _urban.cmd_urbanFlow(null);
                }
                else
                {
                    result = await _urban.cmd_urbanFlow(tag);
                }

                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = $"UrbanDictionary Restult for '{result[2]}'\r\r**Definition**: {result[0]}\r\r**Example**: {result[1]}\r\r**Tags**: {result[3]}"
                };

                await ReplyAsync("", false, builder.Build());
            }
            catch
            {

            }

        }

    }
}
