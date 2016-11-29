using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;
using System.IO;
using NAudio.Wave;

namespace discordMusicBot.src
{
    class downloader
    {
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

            string fileAAC = audio[0].FullName + ".aac";

            string title = audio[0].Title;
            title = title.Remove(title.Length - 10);

            if(File.Exists(workingDir + fileAAC))
            {
                // do nothing
                Console.WriteLine("Info: URL " + url + " was already downloaded, ignoring");
            }
            else
            {
                //download the file
                if (audio.Count > 0)
                {
                    File.WriteAllBytes(workingDir + fileAAC, audio[0].GetBytes());
                    Console.WriteLine("Info: Downloaded " + url );
                }
            }

            string[] returnVar = {
                audio[0].Title,                     //pass the title back
                fileAAC,                            //pass the filename, not sure if we need to retain this
                workingDir + fileAAC,                 //pass the full path to the file to be played back
                audio[0].AudioBitrate.ToString()    //pass the bitrate so we can return the value
            };

            return returnVar;
        }

        public string ConvertAACToWAV(string songTitle, string cacheDir)
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

            return cacheDir+songTitle+".wav";

        }

        //used in _playlist.cmd_plAdd()
        public async Task<string> returnYoutubeTitle(string url)
        {
            var youtube = YouTube.Default;
            var video = youtube.GetAllVideos(url);
            var videoList = video.ToList();

            return videoList[0].Title;
        }


    }
}
