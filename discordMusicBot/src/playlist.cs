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
        public static string npTitle { get; set; }
        public static string npUser { get; set; }
        public static string npUrl { get; set; }
        public static string npSource { get; set; }

        private DiscordClient _client;
        private ModuleManager _manager;
        //private AudioExtensions _audio;

        configuration _config = new configuration();
        downloader _downloader = new downloader();
        player _player = new player();

        public void savePlaylist()
        {
            try
            {
                string loc = "playlist.json";
                string json = JsonConvert.SerializeObject(listLibrary);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving playlist.json.  Error: " + e);
            }
        }

        public void loadPlaylist()
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("Error reading playlist.json.  Error: " + e);
            }
        }

        public void saveBlacklist()
        {
            try
            {
                string loc = "blacklist.json";
                string json = JsonConvert.SerializeObject(listBlacklist, Formatting.Indented);

                if (!File.Exists(loc))
                    File.Create(loc).Close();

                File.WriteAllText(loc, json);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error saving blacklist.json. Error: " + e);
            }

        }

        public void loadBlacklist()
        {
            try
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
            catch(Exception e)
            {
                Console.WriteLine("Error reading blacklist.json.  Error: " + e);
            }
        }

        /// <summary>
        /// Core logic to pick a track from the library
        /// </summary>
        /// <returns>
        ///     null = reroll
        ///     !null = send to player
        /// </returns>
        private string[] getTrack()
        {
            string title = null;
            string user = null;
            string url = null;
            string source = null;

            //1. check if something has been submitted by a user
            string[] trackSubmitted = getTrackSubmitted();

            if (trackSubmitted != null)
            {
                //we have a user file to play
                title = trackSubmitted[0];
                user = trackSubmitted[1];
                url = trackSubmitted[2];
                source = trackSubmitted[3];
            }
            else
            {
                //2. Pick from the Library
                string[] trackLibrary = getTrackLibrary();
                title = trackLibrary[0];
                user = trackLibrary[1];
                url = trackLibrary[2];
                source = trackLibrary[3];
            }

            //3. Check to see if it was blacklisted
            bool blacklist = checkBlacklist(title, url);
            if(blacklist == true)
            {
                //we found a match in the blacklist, need to reroll
                return null;
            }

            //4. Check to see if has been played already
            bool beenPlayed = checkBeenPlayed(title, url);
            if(beenPlayed == true)
            {
                //found a match in the beenPlayed list, need to reroll
                return null;
            }

            //5. Return the value back to be submiited to queue
            string[] returnValue = { title, user, url, source };
            return returnValue;
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
        ///     Checkes to see if the values passed was found on the blacklist
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns>
        ///     returns true if match found
        ///     reutnrs false if no match found
        /// </returns>
        private bool checkBlacklist(string title, string url)
        {
            //check to make sure it wasnt in the blacklist
            //var titleResult = listBlacklist.Find(x => x.title == title);
            var urlResult = listBlacklist.Find(x => x.url == url);

            //if not null, we found a match on the name or the url
            //using try catch given when it parses a null value it hard errors, this catches it and returns the value
            try
            {
                if (urlResult.url != null)
                {
                    return true;
                }

                return false;
            }
            catch
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
        private void addBeenPlayed(string title, string url)
        {
            //get the 10% of the library
            double threshold = listLibrary.Count * 0.1;

            Console.WriteLine("beenPlayed threshhold" + threshold);

            //get the count of items in listBeenPlayed
            if(listBeenPlayed.Count >= threshold)
            {
                //delete the first object
                listBeenPlayed.RemoveAt(0);
            }

            listBeenPlayed.Add(new ListPlaylist
            {
                title = title,
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
            //check to make sure it wasnt in the beenPlayed list
            var urlResult = listSubmitted.Find(x => x.url == url);

            //if not null, we found a match on the name or the url
            //using try catch given when it parses a null value it hard errors, this catches it and returns the value
            try
            {
                if (urlResult.url != null)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }

        }

        public async Task startAutoPlayList(Channel voiceChannel, DiscordClient _client)
        {
            loadPlaylist();
            loadBlacklist();

            bool songActive = true;
            while(songActive == true)
            {
                string[] parsedTrack = getTrack();

                if(parsedTrack == null)
                {
                    //reroll                   
                }
                else
                {
                    //pass off to download the file for cache
                    string[] file = _downloader.download_audio(parsedTrack[2]);

                    _client.SetGame(parsedTrack[0]);

                    //update nowPlaying var for !np
                    npTitle = parsedTrack[0];
                    npUser = parsedTrack[1];
                    npUrl = parsedTrack[2];
                    npSource = parsedTrack[3];

                    await _player.SendAudio(file[2], voiceChannel, songActive, _client);

                    //if a user submitted the song remove it from the disk
                    if(parsedTrack[3] == "Submitted")
                    {
                        File.Delete(file[2]);
                    }

                    addBeenPlayed(parsedTrack[0], parsedTrack[2]);

                }
            }
        } 
        
        /// <summary>
        /// Deperacted
        /// used to pull down the autoplaylist.txt from google drive.
        /// Not needed anymore given all file managment is done in discord.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Used to add a track to playlist.json.
        /// Will not add if dupe url is found.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string cmd_plAdd(string user, string url)
        {   
            //check to see if the url is found in listLibrary
            // -1 means it was not found   
            int urlResult = listLibrary.FindIndex(x => x.url == url);

            if(urlResult == -1)
            {
                downloader _downloader = new downloader();

                //didnt find it already in the list
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
                int urlResult = listLibrary.FindIndex(x => x.url == url);
                if(urlResult >= 0)
                {
                    //found a match
                    listLibrary.RemoveAt(urlResult);
                    savePlaylist();

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
        /// Makes a readable version of the json file.
        /// returns true if it work
        /// returns false if it failed
        /// </summary>
        /// <returns></returns>
        public bool cmd_plexport()
        {
            try
            {
                string loc = "playlist_export.json";
                string json = JsonConvert.SerializeObject(listLibrary, Formatting.Indented);

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
        /// Users to add lines to the blacklist file
        /// </summary>
        /// <param name="user"> username of who sent it</param>
        /// <param name="url"> url of what to blacklist</param>
        /// <returns></returns>
        public string cmd_blAdd(string user, string url)
        {
            downloader _downloader = new downloader();

            int urlResult = listBlacklist.FindIndex(x => x.url == url);

            if(urlResult == -1)
            {
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
                int urlResult = listBlacklist.FindIndex(x => x.url == url);
                if (urlResult >= 0)
                {
                    //found a match
                    listBlacklist.RemoveAt(urlResult);
                    saveBlacklist();

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
        /// returns True if it worked
        /// returns false if failed.
        /// </summary>
        /// <returns></returns>
        public bool cmd_blexport()
        {
            try
            {
                string loc = "blacklist_export.json";
                string json = JsonConvert.SerializeObject(listBlacklist, Formatting.Indented);

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

        /// <summary>
        /// Used to discard the current queue and pick new files.
        /// </summary>
        public string cmd_shuffle()
        {
            //Take the listSubmitted and shuffle it

            if(listSubmitted.Count == 0)
            {
                return "empty";
            }

            Random rng = new Random();
            List<ListPlaylist> temp = new List<ListPlaylist>();

            try
            {
                //loop though all entries of the listSubmitted and shuffle them to a new list
                for (int i = 0; i < listSubmitted.Count; i++)
                {
                    int counter = rng.Next(0, listSubmitted.Count);

                    temp.Add(new ListPlaylist
                    {
                        title = listSubmitted[counter].title,
                        url = listSubmitted[counter].url,
                        user = listSubmitted[counter].user,
                        like = listSubmitted[counter].like,
                        skips = listSubmitted[counter].skips
                    });

                    //remove the current value we have in memory from listSubmitted
                    listSubmitted.RemoveAt(counter);

                }

                //Once all finished merge the data back to listSubmitted and delete temp
                listSubmitted.Clear();
                listSubmitted.AddRange(temp);

                temp.Clear();

                return "true";
            }
            catch(Exception e)
            {
                //something broke
                Console.WriteLine("playlist.cmd_shuffle Error: " + e);
                return "error";
            }
        }

        /// <summary>
        /// Responds with the infomation of the current playing track.
        /// </summary>
        /// <returns></returns>
        public string[] cmd_np()
        {
            string[] value = { npTitle, npUrl, npUser, npSource };
            return value;
        }

        public void cmd_queue()
        {

        }



    }
}
