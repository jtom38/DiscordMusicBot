using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;
using System.IO;

namespace discordMusicBot.src
{
    class downloader
    {
        public string[] download_audio(string url)
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

            string currentDir = Directory.GetCurrentDirectory();
            currentDir = currentDir + "\\cache\\";

            string fileName = audio[0].FullName + ".aac";

            if(File.Exists(currentDir + fileName))
            {
                // do nothing
            }
            else
            {
                //download the file
                if (audio.Count > 0)
                {
                    File.WriteAllBytes(currentDir + fileName, audio[0].GetBytes());
                }
            }

            string[] returnVar = {
                audio[0].Title,                     //pass the title back
                fileName,                           //pass the filename, not sure if we need to retain this
                currentDir + fileName,              //pass the full path to the file to be played back
                audio[0].AudioBitrate.ToString()    //pass the bitrate so we can return the value
            };

            return returnVar;
        }

    }
}
