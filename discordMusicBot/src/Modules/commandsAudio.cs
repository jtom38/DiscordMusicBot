using Discord;
using Discord.Commands;
using discordMusicBot.src.audio;
using discordMusicBot.src.Services;
using discordMusicBot.src.sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace discordMusicBot.src.Modules
{
    public class cmdAudio : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _service;

        private player _player;
        private logs _logs;
        private embed _embed;

        public cmdAudio(AudioService service)
        {
            _service = service;
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("Summon", RunMode = RunMode.Async)]
        [Remarks("Summons the bot to a voice room.")]
        public async Task SummonAsync(IVoiceChannel voiceRoom = null)
        {
            _logs = new logs();
            _embed = new embed();
            try
            {

                // Get the audio channel                
                playlist.voiceRoom = playlist.voiceRoom ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
                if (playlist.voiceRoom == null)
                {
                    await _embed.ErrorEmbedAsync("Summon", "User must be in a voice channel, or a voice channel must be passed as an argument.");
                    return;
                }

                await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

                await _service.AudioLoopAsync(Context.Guild, Context.Channel);

                //await _logs.logMessageAsync("Info", $"{configuration.LoadFile().Prefix}Summon", $"User has summoned the bot to room {Context.Guild.CurrentUser.VoiceChannel}", Context.User.Username);
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", $"{configuration.LoadFile().Prefix}Summon", error.ToString(), Context.User.Username);
            }
        }

    }
}
    

