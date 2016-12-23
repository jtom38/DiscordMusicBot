using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using VideoLibrary;
using System.IO;
using NAudio.Wave;

namespace discordMusicBot.src.audio
{
    class youtube
    {
        logs _logs = new logs();

        public async Task<string[]> download_audio(string url)
        {
            ///<summary>
            /// Documentation on how to use the libVideo
            /// https://github.com/jamesqo/libvideo/blob/master/docs/README.md
            ///</summary>

            var youtube = YouTube.Default;
            var video = youtube.GetAllVideos(url);

            //look at the video and now extract from all the url's the ones that are just audio
            //this way we dont have to do any extraction of video to audio
            var audio = video
                .Where(e => e.AudioFormat == AudioFormat.Aac && e.AdaptiveKind == AdaptiveKind.Audio)
                .ToList();

            string workingDir = Directory.GetCurrentDirectory() + "\\cache\\";

            string fileAAC = audio[0].FullName.Remove(audio[0].FullName.Length - 4 ) + ".aac"; // should remove the .mp4 from the filename and adds aac to the filename
            //string fileAAC = fileName + ".aac"; // 

            string title = audio[0].Title.Remove(audio[0].Title.Length - 10); // removes the " - youtube" part of the string

            if(File.Exists(workingDir + fileAAC))
            {
                // do nothing
                _logs.logMessage("Debug", "downloader.download_audio", "URL " + url + " was already downloaded, ignoring", "system");               
            }
            else
            {
                //download the file
                if (audio.Count > 0)
                {
                    File.WriteAllBytes(workingDir + fileAAC, audio[0].GetBytes());
                    _logs.logMessage("Info", "downloader.download_audio", $"Downloaded {url} to cache.", "system");
                }
            }

            string[] returnVar = {
                audio[0].Title,                     //pass the title back
                fileAAC,                            //pass the filename, not sure if we need to retain this
                workingDir + fileAAC,               //pass the full path to the file to be played back
                audio[0].AudioBitrate.ToString()    //pass the bitrate so we can return the value
            };

            return returnVar;
        }

        public string ConvertAACToWAV(string songTitle, string cacheDir)
        {
            try
            {
                // im going to add this in an atempt to have easier playback though naudio
                // https://stackoverflow.com/questions/13486747/convert-aac-to-wav

                // create media foundation reader to read the AAC encoded file
                using (MediaFoundationReader reader = new MediaFoundationReader(cacheDir + songTitle + ".aac"))
                // resample the file to PCM with same sample rate, channels and bits per sample
                using (ResamplerDmoStream resampledReader = new ResamplerDmoStream(reader,
                    new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels)))
                // create WAVe file
                using (WaveFileWriter waveWriter = new WaveFileWriter(cacheDir + songTitle + ".wav", resampledReader.WaveFormat))
                {
                    // copy samples
                    resampledReader.CopyTo(waveWriter);
                }

                return cacheDir + songTitle + ".wav";
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "downloader.ConvertAACToWAV", error.ToString(), "system");
                return null;
            }
        }

        //used in _playlist.cmd_plAdd()
        public async Task<string[]> returnYoutubeTitle(string url)
        {
            try
            {
                var youtube = YouTube.Default;
                var video = youtube.GetAllVideos(url);
                var videoList = video.ToList();

                string[] returnValue = {
                    videoList[0].Title,
                    videoList[0].FullName
                }; 

                return returnValue;
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "downloader.returnYoutubeTitle", error.ToString(), "system");
                return null;
            }

        }


    }
}
