using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using Discord;
using Discord.Audio;
using NAudio;
using NAudio.Wave;
using System.IO;
using System.Diagnostics.Contracts;

namespace discordMusicBot.src.audio
{
    class player
    {
        /// <summary>
        /// notes from the Discord API group on voice-volume... Still not sure how to use it just yet.
        /// http://hastebin.com/umapabejis.cs
        /// </summary>

        logs _logs = new logs();

        //private DiscordClient _client; //load discord client info
        private IAudioClient _nAudio; //load the discord audio client
        private configuration _config;

        public static bool playingSong = true;

        public static float volume = .10f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath">
        ///     Full filepath needed to track
        /// </param>
        /// <param name="voiceChannel">
        ///     Send the room that the bot is in
        /// </param>
        /// <param name="_client">
        ///     _client
        /// </param>
        /// <returns></returns>
        public async Task SendAudio(string filepath, Channel voiceChannel, DiscordClient _client)
        {
            try
            {
                //try to find a way to tell if she is already in 1. connect to a voice room and 2 in your voice room
                _nAudio = await _client.GetService<AudioService>().Join(voiceChannel);

                playingSong = true;


                var channelCount = _client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.

                using (var MP3Reader = new MediaFoundationReader(filepath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;
                    // Add in the "&& playingSong" so that it only plays while true. For our cheesy skip command.
                    // AGAIN WARNING YOU NEED opus.dll libsodium.dll
                    // If you do not have these, this will not work.
                    try
                    {
                        while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playingSong) // Read audio into our buffer, and keep a loop open while data is present
                        {

                            //adjust volume
                            byte[] adjustedBuffer = ScaleVolumeSafeAllocateBuffers(buffer, volume);

                            if (byteCount < blockSize)
                            {
                                // Incomplete Frame
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            }

                            _nAudio.Send(adjustedBuffer, 0, blockSize); // Send the buffer to Discord
                            
                        }
                        
                    }
                    catch (Exception error)
                    {
                        //await _client.GetService<AudioService>().Join(voiceChannel);
                        Console.WriteLine(error.ToString());
                    }
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "player.sendAudio", error.ToString(), "system");
                //Console.WriteLine("Something went wrong. :(");
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
        public async Task<bool> cmd_stop(DiscordClient _client, Channel voiceRoom)
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

                    _nAudio = await _client.GetService<AudioService>().Join(voiceRoom);
                    _nAudio.Clear();

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
