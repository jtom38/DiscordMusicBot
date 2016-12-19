using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using NAudio;
using NAudio.Wave;
using System.IO;

namespace discordMusicBot.src
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

        //private float volume = .3f;

        //used to play the music to the room
        public async Task SendAudio(string filepath, Channel voiceChannel, DiscordClient _client)
        {
            // When we use the !play command, it'll start this method

            // The comment below is how you'd find the first voice channel on the server "Somewhere"
            //var a = _client.FindServers("Waifu Lounge").
            //var voiceChannel2 = _client.FindServers("Somewhere").FirstOrDefault().VoiceChannels.FirstOrDefault();
            // Since we already know the voice channel, we don't need that.
            // So... join the voice channel:

            //try to find a way to tell if she is already in 1. connect to a voice room and 2 in your voice room

            _nAudio = await _client.GetService<AudioService>().Join(voiceChannel);

            playingSong = true;

            // Simple try and catch.
            try
            {
                using (_client.GetService<AudioService>().Join(voiceChannel))
                {
                    var channelCount = _client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                    var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.

                    //this was moved to test memory leaking
                    using (var MP3Reader = new MediaFoundationReader(filepath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                    {
                        using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                        {
                            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                            int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                            byte[] buffer = new byte[blockSize];
                            int byteCount;
                            // Add in the "&& playingSong" so that it only plays while true. For our cheesy skip command.

                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playingSong == true) // Read audio into our buffer, and keep a loop open while data is present
                            {
                                if(playingSong == false)
                                {
                                    _nAudio.Clear();
                                }
                                else
                                {
                                    if (byteCount < blockSize)
                                    {
                                        // Incomplete Frame
                                        for (int i = byteCount; i < blockSize; i++)
                                            buffer[i] = 0;
                                    }

                                    _nAudio.Send(buffer, 0, blockSize); // Send the buffer to Discord
                                }

                            }
                            
                        }
                    }
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "player.SendAudio", error.ToString(), "system");
            }
            
            _client.Dispose(); //trying this for memory managment. 
            //_nAudio.Clear(); 
        }

        public float VolumeReturn()
        {
            return _config.volume;
        }

        /// <summary>
        ///     Sends the flag to skip the current track
        /// </summary>
        /// <returns>
        ///     return True if it worked
        ///     return false if it failed
        /// </returns>
        public bool cmd_skip()
        {
            try
            {
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
        public bool cmd_stop()
        {
            try
            {
                if (playlist.playlistActive == true)
                {

                    //breaks the loop
                    playlist.playlistActive = false;

                    //forces the current track playing to send the stop command.
                    playingSong = false;

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
        public bool cmd_resume()
        {
            //the autoplayer is turned off
            try
            {
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
