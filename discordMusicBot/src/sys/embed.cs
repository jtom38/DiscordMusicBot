using Discord;
using System.Threading.Tasks;

namespace discordMusicBot.src.sys
{
    class embed
    {

        public async Task<EmbedBuilder> SucessEmbedAsync(string Command, string msg, string Username, string pictureURL = null)
        {
            try
            {
                await Task.Delay(1);

                var builder = new EmbedBuilder()
                {
                    Color = new Color(98, 170, 52),
                    Title = $"{configuration.LoadFile().Prefix}{Command}",
                    Description = $"{Username},\r{msg}.",
                    ImageUrl= pictureURL
                };

                return builder;
            }
            catch
            {
                return null;
            }            
        }

        public async Task<EmbedBuilder> ErrorEmbedAsync(string Command, string msg = null)
        {
            try
            {
                await Task.Delay(1);
                               
                if (msg != null)
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(229, 20, 0),
                        Title = $"{configuration.LoadFile().Prefix}{Command}",
                        Description = $"Oh no... {msg}"
                    };
                    return builder;
                }

                //This is the basic error message
                else if(msg == null)
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(229, 20, 0),
                        Title = $"{configuration.LoadFile().Prefix}{Command}",
                        Description = $"Oh no... something wrong happened. :cry:\rCheck the log for more info."
                    };
                    return builder;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<EmbedBuilder> InfoEmbedAsync(string Command, string msg , string Username)
        {
            try
            {
                await Task.Delay(1);

                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Title = $"{configuration.LoadFile().Prefix}{Command}",
                    Description = $"{Username},\r**Error**\r{msg}."
                };

                return builder;
            }
            catch
            {
                return null;
            }
        }
    }
}
