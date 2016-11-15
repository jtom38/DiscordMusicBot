using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Discord;
using Discord.Audio;
using Discord.Modules;

namespace discordMusicBot.src
{
    public class ListPlaylist
    {
        public string title { get; set; }
        public string user { get; set; }
        public string url { get; set; }       
        public string[] like { get; set; }
        public string[] skips { get; set; }
    }

    class playlist
    {
        public static List<ListPlaylist> listLibrary = new List<ListPlaylist>();        
        public static List<ListPlaylist> listBlacklist = new List<ListPlaylist>();
        public static List<ListPlaylist> listQueue = new List<ListPlaylist>();
        public static List<ListPlaylist> listSubmitted = new List<ListPlaylist>();
        public static List<ListPlaylist> listBeenPlayed = new List<ListPlaylist>();

        private DiscordClient _client;
        private ModuleManager _manager;
        //private AudioExtensions _audio;

        configuration _config = new configuration();
        downloader _downloader = new downloader();
        player _player = new player();

        public void savePlaylist()
        {
            string loc = "playlist.json";
            string json = JsonConvert.SerializeObject(listLibrary);

            if (!File.Exists(loc))
                File.Create(loc).Close();

            File.WriteAllText(loc, json);
        }

        public void loadPlaylist()
        {
            if (File.Exists("playlist.json"))
            {
                string json = File.ReadAllText("playlist.json");

                listLibrary = JsonConvert.DeserializeObject<List<ListPlaylist>>(json);
            }
            else
            {
                savePlaylist();
            }
        }

        public void saveBlacklist()
        {
            string loc = "blacklist.json";
            string json = JsonConvert.SerializeObject(listBlacklist);

            if (!File.Exists(loc))
                File.Create(loc).Close();

            File.WriteAllText(loc, json);
        }

        public void loadBlacklist()
        {
            if (File.Exists("blacklist.json"))
            {
                string json = File.ReadAllText("blacklist.json");

                listBlacklist = JsonConvert.DeserializeObject<List<ListPlaylist>>(json);
            }
            else
            {
                saveBlacklist();
            }

        }

        private void getTrack()
        {
            //1. check if something has been submitted by a user
            string[] trackSubmitted = getTrackSubmitted();

            if (trackSubmitted != null)
            {
                //we have a user file to play
            }

            //2. Pick from the Library
            string[] trackLibrary = getTrackLibrary();
            if (trackLibrary != null)
            {
                //
            }

            //3. Check to see if it was blacklisted

            //4. Check to see if has been played already

            //5. Return the value back to be submiited to queue
        }

        /// <summary>
        /// Picks random tacks from the library and returns the values of the song
        /// </summary>
        /// <returns></returns>
        public string[] getTrackLibrary()
        {
            Random rng = new Random();
            int counter = rng.Next(0, listLibrary.Count);

            string[] value = { listLibrary[counter].title, listLibrary[counter].user, listLibrary[counter].url, "Library" };

            return value;
        }

        /// <summary>
        /// Used to find values if a user submited a song to be played
        /// takes prority over library tracks
        /// </summary>
        /// <returns></returns>
        public string[] getTrackSubmitted()
        {            
            // extract the values
            if(listSubmitted.Count >= 1)
            {
                string[] value = { listLibrary[0].title, listLibrary[0].user, listLibrary[0].url, "Submitted" };
                return value;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Checkes to see if the values passed was found on the blacklist
        /// 
        /// returns true if match found
        /// reutnrs false if no match found
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool checkBlacklist(string title, string url)
        {
            //check to make sure it wasnt in the blacklist
            var titleResult = listBlacklist.Find(x => x.title == title);
            var urlResult = listBlacklist.Find(x => x.url == url);

            //if not null, we found a match on the name or the url
            if(titleResult.title != null || urlResult.url != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds values to the listBeenPlayed so we know what has been done
        /// </summary>
        /// <param name="title"></param>
        /// <param name="user"></param>
        /// <param name="url"></param>
        private void addBeenPlayed(string title, string user, string url)
        {
            //get the 10% of the library
            double threshold = listLibrary.Count * 0.1;

            //get the count of items in listBeenPlayed
            if(listBeenPlayed.Count >= threshold)
            {
                //delete the first object
                listBeenPlayed.RemoveAt(0);
            }

            listBeenPlayed.Add(new ListPlaylist
            {
                title = title,
                user = user,
                url = url
            });

        }
        
        /// <summary>
        /// Checks to see if the track was already played.
        /// 
        /// returns true if match found
        /// reutnr false if not found
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool checkBeenPlayed(string title, string url)
        {
            //check to make sure it wasnt in the blacklist
            var titleResult = listSubmitted.Find(x => x.title == title);
            var urlResult = listSubmitted.Find(x => x.url == url);

            //if not null, we found a match on the name or the url
            if (titleResult.title != null || urlResult.url != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void startAutoPlayList()
        {
            loadPlaylist();
            loadBlacklist();

            bool songActive = true;
            while(songActive == true)
            {
                getTrack();
            }
        } 
        
        public string updatePlaylistFile()
        {
            string returnText = null;

            _config = configuration.LoadFile("config.json");

            //this is slop atm I know.  I need to think of a better way to look for a url compared to a junk text
            if (_config.PlaylistURL.Contains("docs.google.com"))
            {
                //logic
                //check to see if we can get the file first
                //if we have the file good, delete the old one and rename the new file
                //pass to the function to 

                //we have a url
                using (WebClient download = new WebClient())
                {
                    string t = _config.PlaylistURL;
                    try
                    {
                        download.DownloadFile(_config.PlaylistURL, "autoplaylist_download.txt");
                    }
                    catch (Exception e)
                    {
                        //failed to download the file
                        Console.WriteLine("Failed to download autoplaylist.txt");
                        Console.WriteLine(e);

                        returnText = "Failed to download autoplaylist.txt.  Check the console for more info.";
                        return returnText;
                    }
                }

                //we would make it here if we where able to get the file.
                //so we have the new file, purge the old one and rename the downloaded file

                if (File.Exists("autoplaylist.txt"))
                {
                    try
                    {
                        //delete file?
                        File.Delete("autoplaylist.txt");
                                               
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error: Deleting autoplaylist.txt.");
                        Console.WriteLine("Dump info: " + e);

                        returnText = "Error: Failed to delete autoplaylist.txt";
                        return returnText;
                    }
                }

                try
                {
                    File.Move("autoplaylist_download.txt", "autoplaylist.txt");
                    returnText = "Updated the playlist.";
                    return returnText;
                }
                catch
                {
                    //failed to rename the downloaded file.
                    returnText = "Error: Failed to rename autoplaylist.txt";
                    return returnText;
                }
            }
            else
            {
                //we have something wrong with the url
                Console.WriteLine("Error: Failed to check the url in config.json.  Please check the string and try again.");
                returnText = "Error: Failed to check the url in config.json.  Please check the string and try again.";
                return returnText;
            }
        }

        //used to insert lines to the playlist file
        public string cmd_plAdd(string user, string url)
        {
            downloader _downloader = new downloader();

            string title = _downloader.returnYoutubeTitle(url);

            listLibrary.Add(new ListPlaylist
            {
                title = title,
                user = user,
                url = url
            });

            savePlaylist();
            return title;
        }

        /// <summary>
        /// Users to add lines to the blacklist file
        /// </summary>
        /// <param name="user"> username of who sent it</param>
        /// <param name="url"> url of what to blacklist</param>
        /// <returns></returns>
        public string cmd_blacklistAdd(string user, string url)
        {
            downloader _downloader = new downloader();

            string title = _downloader.returnYoutubeTitle(url);

            listBlacklist.Add(new ListPlaylist
            {
                title = title,
                user = user,
                url = url
            });

            saveBlacklist();
            return title;

        }

        /// <summary>
        /// Used to discard the current queue and pick new files.
        /// </summary>
        public void cmd_shuffle()
        {

        }

        /// <summary>
        /// Responds with the infomation of the current playing track.
        /// </summary>
        /// <returns></returns>
        public string[] cmd_np()
        {
            string[] result = { listLibrary[0].title, listLibrary[0].url, listLibrary[0].user };

            return result;
        }

    }
}
