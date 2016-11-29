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
        public static string[] npLike { get; set; }
        public static string[] npSkip { get; set; }

        public static bool libraryLoop = true;
        public static bool playlistActive = true;

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
                string loc = Directory.GetCurrentDirectory() + "\\configs\\playlist.json";
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
                if (File.Exists(Directory.GetCurrentDirectory() + "\\configs\\playlist.json"))
                {
                    string json = File.ReadAllText(Directory.GetCurrentDirectory() + "\\configs\\playlist.json");

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
                string loc = Directory.GetCurrentDirectory() + "\\configs\\blacklist.json";
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
                if (File.Exists(Directory.GetCurrentDirectory() + "\\configs\\blacklist.json"))
                {
                    string json = File.ReadAllText(Directory.GetCurrentDirectory() + "\\configs\\blacklist.json");

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
        private bool getTrack()
        {
            string title = null;
            string user = null;
            string url = null;
            string source = null;

            //1. check if something has been submitted by a user
            string[] trackSubmitted = pickTrackFromSubmitted();

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
                string[] trackLibrary = pickTrackFromLibrary();
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
                return false;
            }

            //4. Check to see if has been played already
            bool beenPlayed = checkBeenPlayed(title, url);
            if(beenPlayed == true)
            {
                //found a match in the beenPlayed list, need to reroll
                return false;
            }

            //5 Need to check Likes

            //6 Need to check skips

            //7. Return the value back to be submiited to queue
            npTitle = title;
            npUser = user;
            npUrl = url;
            npSource = source;
            return true;
        }

        /// <summary>
        /// Used to find values if a user submited a song to be played
        /// takes prority over library tracks
        /// </summary>
        /// <returns></returns>
        public string[] pickTrackFromSubmitted()
        {            
            // extract the values
            if(listSubmitted.Count >= 1)
            {
                string[] value = { listSubmitted[0].title, listSubmitted[0].user, listSubmitted[0].url, "Submitted" };
                return value;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Picks random tacks from the library and returns the values of the song
        /// </summary>
        /// <returns></returns>
        public string[] pickTrackFromLibrary()
        {
            Random rng = new Random();
            int counter = rng.Next(0, listLibrary.Count);

            string[] value = { listLibrary[counter].title, listLibrary[counter].user, listLibrary[counter].url, "Library" };

            return value;
        }

        /// <summary>
        /// Used to remove the url submitted from the listSubmitted queue.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool removeTrackSubmitted(string url)
        {
            try
            {
                var urlResult = listSubmitted.FindIndex(x => x.url == url);
                if(urlResult != -1)
                {
                    //remove the track from the list
                    listSubmitted.RemoveAt(urlResult);
                    return true;
                }
                else
                {
                    // shouldnt even hit this but you know do nothing
                    return false;
                }
            }
            catch
            {
                // something broke removing a track
                return false;
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

            Console.WriteLine("Debug: beenPlayed threshhold " + threshold);

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
            var urlResult = listBeenPlayed.Find(x => x.url == url);

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

        private void checkNumberOfTracksByUserSubmitted(string user)
        {
            var Result = listBeenPlayed.Count(x => x.user == user);
           
            if(Result >= 5)
            {
                //user has subbmitted too many songs
            }

        }

        public async Task startAutoPlayList(Channel voiceChannel, DiscordClient _client)
        {
            loadPlaylist();
            loadBlacklist();

            //library loop is used to keep this loop active
            while(libraryLoop == true)
            {
                //given the loop is always active lets make another loop that we can pause when neede
                while(playlistActive == true)
                {
                    bool result = getTrack();

                    if (result == false)
                    {
                        //reroll                   
                    }
                    else
                    {
                        //pass off to download the file for cache
                        string[] file = await _downloader.download_audio(npUrl);

                        _client.SetGame(npTitle);

                        await _player.SendAudio(file[2], voiceChannel, _client);

                        //if a user submitted the song remove it from the disk
                        if (npSource == "Submitted")
                        {
                            File.Delete(file[2]);
                            removeTrackSubmitted(npUrl);
                        }

                        addBeenPlayed(npTitle, npUrl);

                    }
                }

            }
        } 

        public async Task<string> cmd_play(string url, string user)
        {
            try
            {
                
                string title = await _downloader.returnYoutubeTitle(url);

                //test to see if the url was already blacklisted
                bool blacklistFound = checkBlacklist(title, url);

                //if it wasnt found add it to the queue
                if(blacklistFound != true)
                {
                    listSubmitted.Add(new ListPlaylist
                    {
                        title = title,
                        url = url,
                        user = user
                    });

                    int total = listSubmitted.Count;

                    int position = listSubmitted.FindIndex(x => x.url == url);

                    string value = $"Your request is song number {position + 1}/{total}";

                    return value;
                }
                else
                {
                    //match was found
                    return null;
                }
            }
            catch(Exception e)
            {
                //got a error
                Console.WriteLine($"Error with playlist.cmd_play.  Dump: {e}");
                return null;
            }           
        }
        
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
            int urlResult = listLibrary.FindIndex(x => x.url == url);

            if(urlResult == -1)
            {
                downloader _downloader = new downloader();

                //didnt find it already in the list
                string title = await _downloader.returnYoutubeTitle(url);
                
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
                string loc = Directory.GetCurrentDirectory() + "\\configs\\playlist_export.json";
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
        /// Makes a readable version of the json file.
        /// returns True if it worked
        /// returns false if failed.
        /// </summary>
        /// <returns></returns>
        public bool cmd_blexport()
        {
            try
            {
                string loc = Directory.GetCurrentDirectory() + "\\configs\\blacklist_export.json";
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
        /// Users to add lines to the blacklist file
        /// </summary>
        /// <param name="user"> username of who sent it</param>
        /// <param name="url"> url of what to blacklist</param>
        /// <returns></returns>
        public async Task<string> cmd_blAdd(string user, string url)
        {
            downloader _downloader = new downloader();

            int urlResult = listBlacklist.FindIndex(x => x.url == url);

            if(urlResult == -1)
            {
                string title = await _downloader.returnYoutubeTitle(url);

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

        /// <summary>
        /// Used to display the currently queued up tracks by the users
        /// </summary>
        public string cmd_queue()
        {
            try
            {
                if (listSubmitted.Count >= 1)
                {
                    string result = null;

                    //we have tracks submitted
                    for (int i = 0; i < listSubmitted.Count; i++)
                    {
                        result = result + $"Title: {listSubmitted[i].title} added by {listSubmitted[i].user}\r";
                    }

                    return result;
                }
                else
                {
                    // we have nothing in queue.
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }

    }
}
