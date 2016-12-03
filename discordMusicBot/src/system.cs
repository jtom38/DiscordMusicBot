using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace discordMusicBot.src
{
    class system
    {

        playlist _playlist = new playlist();
        downloader _downloader = new downloader();

        /// <summary>
        /// Used to add a track to playlist.json.
        /// Will not add if dupe url is found.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> cmd_plAdd(string user, string url)
        {
            //check to see if the url is found in listLibrary
            // -1 means it was not found   
            int urlResult = playlist.listLibrary.FindIndex(x => x.url == url);

            if (urlResult == -1)
            {                
                //didnt find it already in the list
                string title = await _downloader.returnYoutubeTitle(url);

                playlist.listLibrary.Add(new ListPlaylist
                {
                    title = title,
                    user = user,
                    url = url
                });

                _playlist.savePlaylist();
                return title;
            }
            else
            {
                //match found, dont add a dupe
                return "dupe";
            }
        }

        /// <summary>
        /// Used to remove a song from the playlist.json
        /// Will check the url to make sure we dont have it in the file already though.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string cmd_plRemove(string url)
        {
            try
            {
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
            catch (Exception e)
            {
                //something went wrong or we didnt find a value in the list.. chances are no value found.
                Console.WriteLine("Error: cmd_plRemove genereted a error.\rError message\r" + e);
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

            int urlResult = playlist.listBlacklist.FindIndex(x => x.url == url);

            if (urlResult == -1)
            {
                string title = await _downloader.returnYoutubeTitle(url);

                playlist.listBlacklist.Add(new ListPlaylist
                {
                    title = title,
                    user = user,
                    url = url
                });

                _playlist.saveBlacklist();
                return title;
            }
            else
            {
                return "dupe";
            }
        }

        /// <summary>
        /// Removes url's from the blacklist if found.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string cmd_blRemove(string url)
        {
            try
            {
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
            catch (Exception e)
            {
                //something went wrong or we didnt find a value in the list.. chances are no value found.
                Console.WriteLine("Error: cmd_blRemove genereted a error.\rError message\r" + e);
                return "error";
            }
        }

        /// <summary>
        /// Makes a readable version of the json file.
        /// returns true if it work
        /// returns false if it failed
        /// </summary>
        /// <returns></returns>
        public bool cmd_plExport()
        {
            try
            {
                string loc = Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json";
                string json = JsonConvert.SerializeObject(playlist.listLibrary, Formatting.Indented);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error running plExport.  Error dump: " + e);
                return false;
            }

        }

        /// <summary>
        /// Makes a readable version of the json file.
        /// returns True if it worked
        /// returns false if failed.
        /// </summary>
        /// <returns></returns>
        public bool cmd_blExport()
        {
            try
            {
                string loc = Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json";
                string json = JsonConvert.SerializeObject(playlist.listBlacklist, Formatting.Indented);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error running blExport.  Error dump: " + e);
                return false;
            }

        }

        
        public bool cmd_exportLog()
        {
            //playlist.listAllSongsPlayed.   

            return false;
        }

    }
}
