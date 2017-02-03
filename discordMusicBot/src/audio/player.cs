using System;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using Discord;
using System.IO;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Discord.Audio;

namespace discordMusicBot.src.audio
{
    class player
    {
        logs _logs = new logs();
        private configuration _config;

        public static IAudioClient audioClient = null;
        public static IVoiceChannel voiceRoom = null;

        public static bool playingSong = true;
        public static float volume = .10f;


        private Process CreateStream(string path)
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                //string newPath = path.Replace(" ", "%20");
                string filePath = $"\"{path}\"";
                var ffmpeg = new ProcessStartInfo
                {
                    FileName = $"{currentDirectory}\\ffmpeg.exe",
                    Arguments = $"-loglevel error -i {filePath} -ac 2 -f s16le -ar 48000 pipe:1 -af 'volume=0.1B'", // -af 'volume = 0.5'
                    UseShellExecute = false,
                    RedirectStandardOutput = true,

                };
                return Process.Start(ffmpeg);
            }
            catch(Exception error)
            {
                Console.WriteLine(error.ToString());
                return null;
            }

        }

        public async Task SendAsync(string path)
        {
            try
            {            
                using (var ffmpeg = CreateStream(path))              
                using (var output = ffmpeg.StandardOutput.BaseStream)                   
                using (var discord = audioClient.CreatePCMStream(1920))
                {
                    await output.CopyToAsync(discord);
                    await discord.FlushAsync();
                }
            }
            catch(Exception error)
            {
                Console.WriteLine(error.ToString());
            }
        }

        public static byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            Contract.Requires(audioSamples != null);
            Contract.Requires(audioSamples.Length % 2 == 0);
            Contract.Requires(volume >= 0f && volume <= 1f);

            var output = new byte[audioSamples.Length];
            if (Math.Abs(volume - 1f) < 0.0001f)
            {
                Buffer.BlockCopy(audioSamples, 0, output, 0, audioSamples.Length);
                return output;
            }

            // 16-bit precision for the multiplication
            int volumeFixed = (int)Math.Round(volume * 65536d);

            for (var i = 0; i < output.Length; i += 2)
            {
                // The cast to short is necessary to get a sign-extending conversion
                int sample = (short)((audioSamples[i + 1] << 8) | audioSamples[i]);
                int processed = (sample * volumeFixed) >> 16;

                output[i] = (byte)processed;
                output[i + 1] = (byte)(processed >> 8);
            }

            return output;
        }

        /// <summary>
        ///     Sends the flag to skip the current track
        /// </summary>
        /// <returns>
        ///     return True if it worked
        ///     return false if it failed
        /// </returns>
        public async Task<bool> cmd_skip()
        {
            try
            {
                await Task.Delay(1);
                if (playingSong == true)
                {
                    playingSong = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "player.cmd_skip", error.ToString(), "system");
                return false;
            }            
        }

        /// <summary>
        ///     Function sends the stop command to the player and autoplay loop.
        /// </summary>
        /// <returns>
        ///     True = Value was changed to stop the loop
        ///     False = The loop wasnt going already
        /// </returns>
        // needs discordClient and Channel
        public async Task<bool> cmd_stop()
        {
            try
            {
                await Task.Delay(1);
                if (playlist.playlistActive == true)
                {

                    //breaks the loop
                    playlist.playlistActive = false;

                    //forces the current track playing to send the stop command.
                    playingSong = false;

                    //_nAudio = await _client.GetService<AudioService>().Join(voiceRoom);
                    //_nAudio.Clear();

                    return true;
                }
                else
                {
                    //not doing anything for a reason.
                    return false;
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "player.cmd_stop", error.ToString(), "system");
                return false;
            }

        }

        /// <summary>
        /// Sends the command to turn the autoplaylist back on.
        /// </summary>
        /// <returns>
        ///     True = turned back on
        ///     False = Either it was on already or error generated
        /// </returns>
        public async Task<bool> cmd_resume()
        {
            //the autoplayer is turned off
            try
            {
                await Task.Delay(1);
                if (playlist.playlistActive == false)
                {
                    //turn it back on.
                    //playlist _playlist = new playlist();
                    playlist.playlistActive = true;
                    return true;
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "player.cmd_stop", error.ToString(), "system");
                return false;
            }

            return false;
        }
    }
}
