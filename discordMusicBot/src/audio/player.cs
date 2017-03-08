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

        public static bool playingSong = true;
        public static float volume = .10f;


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
