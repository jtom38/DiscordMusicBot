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
        public string cmd_play(string url)
        {
            //https://github.com/jamesqo/libvideo/blob/master/docs/README.md

            var youtube = YouTube.Default;
            var video = youtube.GetAllVideos(url);

            var audio = video
                .Where(e => e.AudioFormat == AudioFormat.Aac && e.AdaptiveKind == AdaptiveKind.Audio)
                .ToList();

            if (audio.Count > 0)
            {
                File.WriteAllBytes(@"E:\" + audio[0].Title + ".aac", audio[0].GetBytes());
            }

            return audio[0].Title;
        }

    }
}
