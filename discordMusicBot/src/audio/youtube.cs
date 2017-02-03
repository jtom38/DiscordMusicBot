using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;
using VideoLibrary;
using System.IO;
using NAudio.Wave;
using System.Diagnostics;

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

            try
            {
                await Task.Delay(1);
                var youtube = YouTube.Default;
                var video = youtube.GetAllVideos(url);

                //look at the video and now extract from all the url's the ones that are just audio
                //this way we dont have to do any extraction of video to audio
                var audio = video
                    .Where(e => e.AudioFormat == AudioFormat.Aac && e.AdaptiveKind == AdaptiveKind.Audio)
                    .ToList();

                string workingDir = Directory.GetCurrentDirectory() + "\\cache\\";

                string title = audio[0].Title.Remove(audio[0].Title.Length - 10); // removes the " - youtube" part of the string

                string fileName = audio[0].FullName.Remove(audio[0].FullName.Length - 14); // should remove the .mp4 from the filename and adds aac to the filename

                string fullFileNameAAC = workingDir + fileName + ".aac";
                string fileNameAAC = fileName + ".aac";

                string FullFileNameMP3 = workingDir + fileName + ".mp3";
                string fileNameMP3 = fileName + ".mp3";

                if (File.Exists(FullFileNameMP3))
                {
                    // do nothing
                    await _logs.logMessageAsync("Debug", "downloader.download_audio", $"URL: {url} was already downloaded, ignoring", "system");
                }
                else
                {
                    //download the file
                    if (audio.Count > 0)
                    {
                        try
                        {
                            File.WriteAllBytes(fullFileNameAAC, audio[0].GetBytes());
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine(error.ToString());
                        }
                        
                        ConvertToMp3(fullFileNameAAC, FullFileNameMP3).WaitForExit();

                        File.Delete(fullFileNameAAC);

                        await _logs.logMessageAsync("Info", "downloader.download_audio", $"Downloaded {url} to cache.", "system");
                    }
                }

                string[] returnVar =
                {
                        audio[0].Title,                     //pass the title back
                        FullFileNameMP3,                            //pass the filename, not sure if we need to retain this
                        fileNameMP3,               //pass the full path to the file to be played back
                        audio[0].AudioBitrate.ToString()    //pass the bitrate so we can return the value
                };

                return returnVar;
            }
            catch (Exception error)
            {

                await _logs.logMessageAsync("Error", "youtube.download_audio", error.ToString(), "system");
                return null;
            }

        }

        private Process ConvertToMp3(string source, string destination)
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string sourceFilePath = "\"" +source+ "\"";
                string destFilePath = "\"" + destination + "\"";
                var ffmpeg = new ProcessStartInfo
                {
                    FileName = $"{currentDirectory}\\ffmpeg.exe",
                    Arguments = $"-loglevel error -i {sourceFilePath} -codec:a libmp3lame -qscale:a 2 {destFilePath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                };               
                return Process.Start(ffmpeg);
            }
            catch
            {
                return null;
            }

        }

        //used in _playlist.cmd_plAdd()
        public async Task<string[]> returnYoutubeTitle(string url)
        {
            try
            {
                await Task.Delay(1);

                var youtube = YouTube.Default;
                var video = youtube.GetAllVideos(url);
                //var videoList = video.ToList();
                var videoList = video.FirstOrDefault();


                string title = videoList.Title.Remove(videoList.Title.Length - 10); // removes the " - youtube" part of the string

                string[] returnValue = {
                    title,
                    videoList.FullName
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
