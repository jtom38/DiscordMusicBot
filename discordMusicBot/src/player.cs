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
        //private DiscordClient _client; //load discord client info
        private IAudioClient _nAudio; //load naudio client
        private configuration _config;

        private float volume = .3f;

        //used to play the music to the room
        public async Task SendAudio(string filepath, Channel voiceChannel, bool playingSong, DiscordClient _client)
        {
            // When we use the !play command, it'll start this method

            // The comment below is how you'd find the first voice channel on the server "Somewhere"
            //var a = _client.FindServers("Waifu Lounge").
            //var voiceChannel2 = _client.FindServers("Somewhere").FirstOrDefault().VoiceChannels.FirstOrDefault();
            // Since we already know the voice channel, we don't need that.
            // So... join the voice channel:

            //try to find a way to tell if she is already in 1. connect to a voice room and 2 in your voice room

            _nAudio = await _client.GetService<AudioService>().Join(voiceChannel);

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
                            // AGAIN
                            // WARNING
                            // YOU NEED
                            // vvvvvvvvvvvvvvv
                            // opus.dll
                            // libsodium.dll
                            // ^^^^^^^^^^^^^^^
                            // If you do not have these, this will not work.
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playingSong) // Read audio into our buffer, and keep a loop open while data is present
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
            catch(Exception e)
            {
                System.Console.WriteLine("Something went wrong. :(\rDump: " + e);
            }
            
            _client.Dispose(); //trying this for memory managment. 
            //_nAudio.Clear(); 
        }

        public float VolumeReturn()
        {
            return _config.volume;
        }

    }
}
