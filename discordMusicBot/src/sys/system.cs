using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Discord;
using discordMusicBot.src.audio;

namespace discordMusicBot.src.sys
{
    class system
    {

        playlist _playlist = new playlist();
        youtube _downloader = new youtube();
        logs _logs = new logs();

        /// <summary>
        /// Used to add a track to playlist.json.
        /// Will not add if dupe url is found.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> cmd_plAdd(string user, string url)
        {
            try
            {
                //check to see if the url is found in listLibrary
                // -1 means it was not found   
                int urlResult = playlist.listLibrary.FindIndex(x => x.url == url);

                if (urlResult == -1)
                {
                    //didnt find it already in the list
                    string[] title = await _downloader.returnYoutubeTitle(url);

                    //adds to the library
                    playlist.listLibrary.Add(new ListPlaylist
                    {
                        title = title[0],
                        user = user,
                        url = url,
                        filename = title[1]
                    });

                    _playlist.savePlaylist();

                    //add to the current playing queue.
                    playlist.listAutoQueue.Add(new ListPlaylist
                    {
                        title = title[0],
                        user = user,
                        url = url,
                        filename = title[1]
                    });
                    return title[0];
                }
                else
                {
                    //match found, dont add a dupe
                    return "dupe";
                }
            }catch(Exception error)
            {
                _logs.logMessage("Error", "system.cmd_play", error.ToString(), "system");
                return null;
            }

        }

        /// <summary>
        /// Used to remove a song from the playlist.json
        /// Will check the url to make sure we dont have it in the file already though.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> cmd_plRemove(string url)
        {
            try
            {
                await Task.Delay(1);

                int urlResult = playlist.listLibrary.FindIndex(x => x.url == url);
                if (urlResult >= 0)
                {
                    //found a match
                    playlist.listLibrary.RemoveAt(urlResult);
                    _playlist.savePlaylist();

                    return "match";
                }
                else
                {
                    //this should be -1 means no match found
                    return "noMatch";
                }
            }
            catch (Exception error)
            {
                //something went wrong or we didnt find a value in the list.. chances are no value found.
                _logs.logMessage("Error", "system.cmd_plRemove", error.ToString(), "system");
                return "error";
            }

        }

        /// <summary>
        /// Users to add lines to the blacklist file
        /// </summary>
        /// <param name="user"> username of who sent it</param>
        /// <param name="url"> url of what to blacklist</param>
        /// <returns></returns>
        public async Task<string> cmd_blAdd(string user, string url)
        {
            try
            {
                int urlResult = playlist.listBlacklist.FindIndex(x => x.url == url);

                if (urlResult == -1)
                {
                    string[] title = await _downloader.returnYoutubeTitle(url);

                    playlist.listBlacklist.Add(new ListPlaylist
                    {
                        title = title[0],
                        user = user,
                        url = url
                    });

                    _playlist.saveBlacklist();
                    return title[0];
                }
                else
                {
                    return "dupe";
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "system.cmd_blAdd", error.ToString(), "system");
                return "error";
            }

        }

        /// <summary>
        /// Removes url's from the blacklist if found.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> cmd_blRemove(string url)
        {
            try
            {
                await Task.Delay(1);

                int urlResult = playlist.listBlacklist.FindIndex(x => x.url == url);
                if (urlResult >= 0)
                {
                    //found a match
                    playlist.listBlacklist.RemoveAt(urlResult);
                    _playlist.saveBlacklist();

                    return "match";
                }
                else
                {
                    //this should be -1 means no match found
                    return "noMatch";
                }
            }
            catch (Exception error)
            {
                //something went wrong or we didnt find a value in the list.. chances are no value found.
                _logs.logMessage("Error", "system.cmd_blRemove", error.ToString(), "system");
                return "error";
            }
        }

        /// <summary>
        /// Makes a readable version of the json file.
        /// returns true if it work
        /// returns false if it failed
        /// </summary>
        /// <returns></returns>
        public async Task<bool> cmd_plExport()
        {
            try
            {
                await Task.Delay(1);

                string loc = Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json";
                string json = JsonConvert.SerializeObject(playlist.listLibrary, Formatting.Indented);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);

                return true;
            }
            catch (Exception error)
            {               
                _logs.logMessage("Error", "system.cmd_plExport", error.ToString(), "system");
                return false;
            }

        }

        /// <summary>
        /// Makes a readable version of the json file.
        /// returns True if it worked
        /// returns false if failed.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> cmd_blExport()
        {
            try
            {
                await Task.Delay(1);

                string loc = Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json";
                string json = JsonConvert.SerializeObject(playlist.listBlacklist, Formatting.Indented);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);

                return true;
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "system.cmd_blExport", error.ToString(), "system");
                return false;
            }

        }
      
    }
}
