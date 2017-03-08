using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Audio;
using discordMusicBot.src.audio;
using discordMusicBot.src.sys;
using System;

namespace discordMusicBot.src.Services
{
    public class AudioService
    {
        private playlist _playlist = new playlist();
        private logs _logs = new logs();
        private youtube _youtube = new youtube();

        public static string npTitle = null;
        public static string npUser = null;
        public static string npUrl = null;
        public static string npSource = null;
        public static string[] npLike = null;
        public static string[] npSkip = null;
        public static string npFileName = null;
        public static bool npDeleteAfterPlaying = false;

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.").ConfigureAwait(false);
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.DisconnectAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.").ConfigureAwait(false);
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}").ConfigureAwait(false);
                var output = CreateStream(path).StandardOutput.BaseStream;
                var stream = client.CreatePCMStream(1920);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        private Process CreateStream(string path)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
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

        public async Task AudioLoopAsync(IGuild guild, IMessageChannel channel)
        {
            try
            {
                
                while (playlist.playlistActive == true)
                {
                    //reset the nowplaying vars given a new song is being picked
                    await ResetNPValues();

                    if (playlist.listSubmitted.Count >= 1) //check to see if someone has something queued up in submmitted
                        await pickTrackFromSubmitted();
                    else if (playlist.listSubmitted.Count == 0) //if nothing is found go back to the listAutoQueue
                        await getAutoQueueTrackInfo();

                    string filePath = await getFileNameToPlay(); // get the file name that we are going to pass to the player

                    //await _client.SetGameAsync(npTitle); //update the track that is currently playing

                    await _logs.logMessageAsync("Info", "playlist.playAutoQueue", $"Track:'{npTitle}' was sent to the audio player.", "system");

                    await SendAudioAsync(guild, channel, filePath);

                    await removeTrackPlayed(filePath); //if a user submitted the song remove it from the disk
                }
            }
            catch
            {

            }
        }

        private async Task ResetNPValues()
        {
            try
            {
                
                await Task.Delay(1);
                npUrl = null;
                npTitle = null;
                npUser = null;
                npSource = null;
                npLike = null;
                npSkip = null;
                npFileName = null;
                npDeleteAfterPlaying = false;
            }
            catch
            {

            }
        }

        /// <summary>
        /// Used to find values if a user submited a song to be played
        /// takes prority over library tracks
        /// </summary>
        /// <returns></returns>
        private async Task<bool> pickTrackFromSubmitted()
        {
            _logs = new logs();

            try
            {
                // extract the values
                if (playlist.listSubmitted.Count >= 1)
                {
                    int t = playlist.listLibrary.FindIndex(x => x.url == playlist.listSubmitted[0].url);
                    if (t != -1)
                    {
                        //Track was found in the Library
                        npFileName = playlist.listSubmitted[0].filename;
                       npDeleteAfterPlaying = false;
                    }
                    else
                    {
                        //Not found in the library
                        npFileName = null;
                        npDeleteAfterPlaying = true;
                    }

                    npTitle = playlist.listSubmitted[0].title;
                    npUrl = playlist.listSubmitted[0].url;
                    npUser = playlist.listSubmitted[0].user;
                    npSource = "Submitted";
                    npLike = playlist.listSubmitted[0].like;
                    npSkip = playlist.listSubmitted[0].skips;

                    _logs.logMessage("Debug", "AudioService.pickTrackFromSubmitted", $"Title: {npTitle} was picked from listSubmitted.", "System");
                    await Task.Delay(1);
                    return true;
                }
                else
                {
                    //shouldnt hit this ever
                    _logs.logMessage("Debug", "AudioService.pickTrackFromSubmitted", "Function was hit even though nothing was in queue", "System");
                    await Task.Delay(1);
                    return false;
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "AudioService.pickTrackFromSubmitted", error.ToString(), "System");
                await Task.Delay(1);
                return false;
            }
        }

        private async Task<bool> getAutoQueueTrackInfo()
        {
            try
            {
                if (playlist.listAutoQueue.Count >= 1)
                {
                    //push the current track in line to the nowplaying vars
                    npTitle = playlist.listAutoQueue[0].title;
                    npUrl = playlist.listAutoQueue[0].url;
                    npUser = playlist.listAutoQueue[0].user;
                    npSource = "Library";
                    npSkip = playlist.listAutoQueue[0].skips;
                    npLike = playlist.listAutoQueue[0].like;
                    npFileName = playlist.listAutoQueue[0].filename;

                    _logs.logMessage("Debug", "playlist.getAutoQueueTrackInfo", $"URL: {npUrl} was picked to be played.", "system");
                    await Task.Delay(1);
                    return true;
                }
                await Task.Delay(1);
                return false;
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "playlist.getAutoQueueTrackInfo", error.ToString(), "system");
                await Task.Delay(1);
                return false;
            }
        }

        private async Task<string> getFileNameToPlay()
        {
            try
            {
                //pass off to download the file for cache
                string filePath = Directory.GetCurrentDirectory() + "\\cache\\";

                if (npFileName == null)
                {
                    string[] file = await _youtube.download_audio(npUrl);
                    filePath = filePath + file[1];
                    npFileName = file[1];

                    //need to write the data to the listLibrary with the new fileName so we avoid downloading again 
                    await updateFileNameInTheLibrary();

                    return filePath;
                }
                else
                {
                    filePath = filePath + npFileName;
                    if (!File.Exists(filePath)) //check to make sure the file is still on the disk.
                    {
                        //if we cant find the file for some reason, go download it again.
                        string[] file = await _youtube.download_audio(npUrl);
                        filePath = file[1];
                        npFileName = file[1];

                        npTitle = file[0];

                        //need to write the data to the listLibrary with the new fileName so we avoid downloading again 
                        await updateFileNameInTheLibrary();

                        return filePath;
                    }

                    return filePath;
                }
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", "playlist.getFileNameToPlay", error.ToString(), "system");
                return null;
            }
        }

        private async Task updateFileNameInTheLibrary()
        {
            try
            {
                await Task.Delay(1);

                // take the title stored in memory and find the record in
                var Result = playlist.listLibrary.FindIndex(x => x.title == npTitle);

                if (Result != -1)
                {
                    if (playlist.listLibrary[Result].filename == null)
                    {
                        playlist.listLibrary[Result].filename = npFileName;
                        _playlist.savePlaylist();
                    }

                }
                else
                {
                   await _logs.logMessageAsync("Error", "playlist.updateFileNameInTheLibrary", $"Unable to update {npTitle}.  Unable to find the index number for the requested title.", "system");
                }

            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", "playlist.updateFileNameInTheLibrary", error.ToString(), "system");
            }
        }

        /// <summary>
        /// Figures out what to do with the track now that it has been played.
        /// </summary>
        /// <param name="filePath"></param>
        private async Task<bool> removeTrackPlayed(string filePath)
        {
            try
            {
                if (npSource == "Submitted")
                {
                    await removeTrackSubmitted(npUrl);

                    if (npDeleteAfterPlaying == true)
                    {
                        File.Delete(filePath);
                    }
                }
                else if (npSource == "Library")
                {
                    await moveAutoQueueTrackPlayedToBackOfQueue();
                }
                await Task.Delay(1);
                return true;
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", "playlist.removeTrackPlayed", error.ToString(), "system");
                return false;
            }
        }

        /// <summary>
        /// Used to remove the url submitted from the listSubmitted queue.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> removeTrackSubmitted(string url)
        {
            try
            {
                await Task.Delay(1);
                var urlResult = playlist.listSubmitted.FindIndex(x => x.url == url);
                if (urlResult != -1)
                {
                    //remove the track from the list
                    playlist.listSubmitted.RemoveAt(urlResult);
                    return true;
                }
                else
                {
                    // shouldnt even hit this but you know do nothing
                    return false;
                }
            }
            catch (Exception error)
            {
                // something broke removing a track
                _logs.logMessage("Error", "playlist.removeTrackSubmitted", error.ToString(), "system");
                return false;
            }

        }

        private async Task<bool> moveAutoQueueTrackPlayedToBackOfQueue()
        {
            try
            {
                await Task.Delay(1);
                //get infomation for line 0 in memory
                string title = playlist.listAutoQueue[0].title;
                string url = playlist.listAutoQueue[0].url;
                string user = playlist.listAutoQueue[0].user;
                string[] skip = playlist.listAutoQueue[0].skips;
                string[] like = playlist.listAutoQueue[0].like;
                string fileName = playlist.listAutoQueue[0].filename;

                //remove from line 0
                playlist.listAutoQueue.RemoveAt(0);

                //push to back of queue
                playlist.listAutoQueue.Add(new ListPlaylist
                {
                    title = title,
                    url = url,
                    user = user,
                    skips = skip,
                    like = like,
                    filename = fileName
                });

                await _logs.logMessageAsync("Debug", "playlist.moveAutoQueueTrackPlayedToBackOfQueue", $"URL: {url} was moved to the back of the queue.", "system");
                return true;
            }
            catch (Exception error)
            {
                await _logs.logMessageAsync("Error", "playlist.moveAutoQueueTrackPlayedToBackOfQueue", error.ToString(), "system");
                return false;
            }
        }
    }

}
