using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using discordMusicBot.src.sys;
using Discord;
using Discord.Modules;
using System.Diagnostics;

namespace discordMusicBot.src.audio
{
    public class ListPlaylist
    {
        public string title { get; set; }
        public string user { get; set; }
        public string url { get; set; }       
        public string[] like { get; set; }
        public string[] skips { get; set; }
        public string filename { get; set; }
    }

    class playlist
    {
        public static List<ListPlaylist> listLibrary = new List<ListPlaylist>();        
        public static List<ListPlaylist> listBlacklist = new List<ListPlaylist>();
        public static List<ListPlaylist> listQueue = new List<ListPlaylist>();
        public static List<ListPlaylist> listSubmitted = new List<ListPlaylist>();
        public static List<ListPlaylist> listBeenPlayed = new List<ListPlaylist>();
        public static List<ListPlaylist> listAllSongsPlayed = new List<ListPlaylist>();
        public static List<ListPlaylist> listAutoQueue = new List<ListPlaylist>();
        public static string npTitle { get; set; }
        public static string npUser { get; set; }
        public static string npUrl { get; set; }
        public static string npSource { get; set; }
        public static string[] npLike { get; set; }
        public static string[] npSkip { get; set; }
        public static string npFileName { get; set; }
        public static bool npDeleteAfterPlaying = false;

        static string playlistFile = Directory.GetCurrentDirectory() + "\\configs\\playlist.json";
        static string blacklistFile = Directory.GetCurrentDirectory() + "\\configs\\blacklist.json";

        public static bool libraryLoop = true;
        public static bool playlistActive = true;

        private DiscordClient _client;
        private ModuleManager _manager;
        //private AudioExtensions _audio;

        configuration _config = new configuration();
        youtube _downloader = new youtube();
        player _player = new player();
        logs _logs = new logs();

        public void savePlaylist()
        {
            try
            {
                //string loc = Directory.GetCurrentDirectory() + "\\configs\\playlist.json";
                string json = JsonConvert.SerializeObject(listLibrary);

                if (!File.Exists(playlistFile))
                    File.Create(playlistFile).Close();

                File.WriteAllText(playlistFile, json);
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
                if (File.Exists(playlistFile))
                {
                    string json = File.ReadAllText(playlistFile);

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
                //string loc = Directory.GetCurrentDirectory() + "\\configs\\blacklist.json";
                string json = JsonConvert.SerializeObject(listBlacklist, Formatting.Indented);

                if (!File.Exists(blacklistFile))
                    File.Create(blacklistFile).Close();

                File.WriteAllText(blacklistFile, json);
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
                if (File.Exists(blacklistFile))
                {
                    string json = File.ReadAllText(blacklistFile);

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

        public void shuffleLibrary()
        {
            Stopwatch stopWatch = new Stopwatch(); //make the stopwatch so we can track how long it takes
            try
            {
                loadPlaylist(); //load the playlist to memory
                loadBlacklist(); //load the blacklist to memory

                _logs.logMessage("Info", "playlist.shuffleLibrary", $"Starting shuffle of {listLibrary.Count} tracks", "system");

                Random rng = new Random();

                List<ListPlaylist> tempLibrary = new List<ListPlaylist>();
                tempLibrary.AddRange(listLibrary);

                stopWatch.Start();
                //generate a temp lis
                for (int i = 0; i < listLibrary.Count; i++)
                {

                    int counter = rng.Next(0, tempLibrary.Count);

                    listAutoQueue.Add(new ListPlaylist
                    {
                        title = tempLibrary[counter].title,
                        url = tempLibrary[counter].url,
                        user = tempLibrary[counter].user,
                        like = tempLibrary[counter].like,
                        skips = tempLibrary[counter].skips,
                        filename = tempLibrary[counter].filename                        
                    });

                    //remove the item we just added to the queue
                    tempLibrary.RemoveAt(counter);

                }
                stopWatch.Stop();
                _logs.logMessage("Info", "playlist.shuffleLibrary", $"Finished shuffle of {listLibrary.Count} tracks in {stopWatch.Elapsed.TotalSeconds}s", "system");
            }
            catch (Exception error)
            {
                stopWatch.Stop();
                _logs.logMessage("Error", "playlist.shuffleLibrary", error.ToString(), "system");
            }

        }

        public async Task playAutoQueue(Channel voiceChannel, DiscordClient _client)
        {
            try
            {
                //library loop is used to keep this loop active
                while (libraryLoop == true)
                {
                    //given the loop is always active lets make another loop that we can pause when needed
                    while (playlistActive == true)
                    {

                        //reset the nowplaying vars given a new song is being picked
                        npUrl = null;
                        npTitle = null;
                        npUser = null;
                        npSource = null;
                        npLike = null;
                        npSkip = null;
                        npFileName = null;
                        npDeleteAfterPlaying = false;

                        //check to see if someone has something queued up in submmitted
                        if (listSubmitted.Count >= 1)
                        {
                            pickTrackFromSubmitted();
                        }
                        else
                        {
                            //if nothing is found go back to the listAutoQueue
                            getAutoQueueTrackInfo();
                        }

                        //pass off to download the file for cache
                        string filePath = null;
                        if(npFileName == null)
                        {
                            string[] file = await _downloader.download_audio(npUrl);
                            filePath = Directory.GetCurrentDirectory() + "\\cache\\" + file[1];
                            npFileName = file[1];

                            //need to write the data to the listLibrary with the new fileName so we avoid downloading again 
                            updateFileNameInTheLibrary();
                        }
                        else
                        {
                            filePath = Directory.GetCurrentDirectory() + "\\cache\\" + npFileName;
                            if (!File.Exists(filePath)) //check to make sure the file is still on the disk.
                            {
                                //if we cant find the file for some reason, go download it again.
                                string[] file = await _downloader.download_audio(npUrl);
                                filePath = Directory.GetCurrentDirectory() + "\\cache\\" + file[1];
                                npFileName = file[1];

                                //need to write the data to the listLibrary with the new fileName so we avoid downloading again 
                                updateFileNameInTheLibrary();
                            }

                        }                      
                        
                        _client.SetGame(npTitle);

                        _logs.logMessage("Info", "playlist.playAutoQueue", $"Track:'{npTitle}' was sent to the audio player.", "system");

                        await _player.SendAudio(filePath, voiceChannel, _client); //send the file and functions over to the audio player to send to the server

                        //if a user submitted the song remove it from the disk
                        removeTrackPlayed(filePath);

                    }
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "playlist.playAutoQueue", error.ToString(), "system");
            }
        }

        private void getAutoQueueTrackInfo()
        {
            try
            {
                if(listAutoQueue.Count >= 1)
                {
                    //push the current track in line to the nowplaying vars
                    npTitle = listAutoQueue[0].title;
                    npUrl = listAutoQueue[0].url;
                    npUser = listAutoQueue[0].user;
                    npSource = "Library";
                    npSkip = listAutoQueue[0].skips;
                    npLike = listAutoQueue[0].like;
                    npFileName = listAutoQueue[0].filename;

                    _logs.logMessage("Debug", "playlist.getAutoQueueTrackInfo", $"URL: {npUrl} was picked to be played.", "system");
                }

            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "playlist.getAutoQueueTrackInfo", error.ToString(), "system");
            }
        }

        private void moveAutoQueueTrackPlayedToBackOfQueue()
        {
            try
            {
                //get infomation for line 0 in memory
                string title = listAutoQueue[0].title;
                string url = listAutoQueue[0].url;
                string user = listAutoQueue[0].user;
                string[] skip = listAutoQueue[0].skips;
                string[] like = listAutoQueue[0].like;
                string fileName = listAutoQueue[0].filename;

                //remove from line 0
                listAutoQueue.RemoveAt(0);

                //push to back of queue
                listAutoQueue.Add(new ListPlaylist
                {
                    title = title,
                    url = url,
                    user = user,
                    skips = skip,
                    like = like,
                    filename = fileName
                });

                _logs.logMessage("Debug", "playlist.moveAutoQueueTrackPlayedToBackOfQueue", $"URL: {url} was moved to the back of the queue.", "system");
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "playlist.moveAutoQueueTrackPlayedToBackOfQueue", error.ToString(), "system");
            }
        }

        public void getTrackFromSubmittedQueue()
        {
            try
            {
                if(listSubmitted.Count >= 1)
                {
                    npTitle = listSubmitted[0].title;
                    npUrl = listSubmitted[0].url;
                    npUser = listSubmitted[0].user;
                    npSource = "Submitted";
                    npSkip = listSubmitted[0].skips;
                    npLike = listSubmitted[0].like;

                    _logs.logMessage("Debug", "playlist.getTrackFromSubmittedQueue", $"Url: {npUrl} was picked from the submitted queue.", "system");
                }               
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "playlist.getTrackFromSubmittedQueue", error.ToString(), "system");
            }
        }

        private void updateFileNameInTheLibrary()
        {
            try
            {
                // take the title stored in memory and find the record in
                var Result = listLibrary.FindIndex(x => x.title == npTitle);

                if(Result != -1)
                {
                    if(listLibrary[Result].filename == null)
                    {
                        listLibrary[Result].filename = npFileName;
                        savePlaylist();
                    }
                    
                }
                else
                {
                    _logs.logMessage("Error", "playlist.updateFileNameInTheLibrary", $"Unable to update {npTitle}.  Unable to find the index number for the requested title.", "system");
                    return;
                }
                
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.updateFileNameInTheLibrary", error.ToString(), "system");
            }
        }
        
        /// <summary>
        /// Used to find values if a user submited a song to be played
        /// takes prority over library tracks
        /// </summary>
        /// <returns></returns>
        public void pickTrackFromSubmitted()
        {
            try
            {
                // extract the values
                if (listSubmitted.Count >= 1)
                {
                    int t = listLibrary.FindIndex(x => x.url == listSubmitted[0].url);
                    if(t != -1)
                    {
                        //Track was found in the Library
                        npFileName = listSubmitted[0].filename;
                        npDeleteAfterPlaying = false;
                    }
                    else
                    {
                        //Not found in the library
                        npFileName = null;
                        npDeleteAfterPlaying = true;
                    }

                    npTitle = listSubmitted[0].title;
                    npUrl = listSubmitted[0].url;
                    npUser = listSubmitted[0].user;
                    npSource = "Submitted";
                    npLike = listSubmitted[0].like;
                    npSkip = listSubmitted[0].skips;

                    _logs.logMessage("Debug", "playlist.pickTrackFromSubmitted", $"Title: {npTitle} was picked from listSubmitted.", "System");
                }
                else
                {
                    //shouldnt hit this ever
                    _logs.logMessage("Debug", "playlist.pickTrackFromSubmitted", "Function was hit even though nothing was in queue", "System");
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.pickTrackFromSubmitted", error.ToString(), "System");
            }     


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
        /// Checks to see if a user can submit another track
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        ///    -1 = Error generated
        ///     0 = Too many tracks submitted
        ///     1 = Okay to add another track to queue
        /// </returns>
        public int checkNumberOfTracksByUserSubmitted(string user)
        {
            try
            {
                var Result = listSubmitted.Count(x => x.user == user);

                if (Result >= _config.maxTrackSubmitted)
                {
                    //user has subbmitted too many songs
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception error)
            {
                return -1;
            }
        } 

        /// <summary>
        /// Figures out what to do with the track now that it has been played.
        /// </summary>
        /// <param name="filePath"></param>
        private void removeTrackPlayed(string filePath)
        {
            try
            {
                if(npSource == "Submitted")
                {
                    removeTrackSubmitted(npUrl);

                    if (npDeleteAfterPlaying == true)
                    {
                        File.Delete(filePath);
                    }
                }
                else if(npSource == "Library")
                {
                    moveAutoQueueTrackPlayedToBackOfQueue();
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.removeTrackPlayed", error.ToString(), "system");
            }
        }

        public async Task<string> cmd_play(string url, string user)
        {
            try
            {

                //check to see if the track might be in the library already
                var urlResult = listLibrary.FindIndex(x => x.url == url);
                if (urlResult != -1)
                {
                    listSubmitted.Add(new ListPlaylist
                    {
                        title = listLibrary[urlResult].title,
                        url = listLibrary[urlResult].url,
                        user = user,

                    });

                    string value = $"{user},\rYour track request of {listLibrary[urlResult].title} has been submitted.\rTracks in queue: {listSubmitted.Count}";
                    return value;
                }
                else
                {
                    string[] title = await _downloader.returnYoutubeTitle(url);

                    //check to see if the song requested was played already and in the listSubmitted
                    //this is used to deal with a issue discovered when testing a file playback error.
                    //b.0005
                    int beenPlayedPosition = listBeenPlayed.FindIndex(x => x.url == url);
                    if (beenPlayedPosition != -1)
                    {
                        listSubmitted.RemoveAt(beenPlayedPosition);
                    }

                    //test to see if the url was already blacklisted
                    bool blacklistFound = checkBlacklist(title[0], url);

                    //if it wasnt found add it to the queue
                    if (blacklistFound != true)
                    {
                        listSubmitted.Add(new ListPlaylist
                        {
                            title = title[0],
                            url = url,
                            user = user
                        });

                        string value = $"{user},\rYour track request of {title[0]} has been submitted.\rTracks in queue: {listSubmitted.Count}";

                        return value;
                    }
                    else
                    {
                        //match was found
                        return null;
                    }
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
            try
            {
                string[] value = { npTitle, npUrl, npUser, npSource };
                return value;

            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.cmd_np", error.ToString(), "system");
                return null;
            }
;
        }

        /// <summary>
        /// Used to remove the currently playing track from the library based off whats in the NowPlaying variables
        /// </summary>
        public bool cmd_npRemove()
        {
            try
            {
                //get the info from the vars
                if (npUrl != null)
                {
                    var urlResult = listLibrary.FindIndex(x => x.url == npUrl);

                    if (urlResult != -1)
                    {
                        listLibrary.RemoveAt(urlResult); //remove the value from the list
                        savePlaylist(); //save the change
                        

                        return true;
                    }
                    else
                    {
                        _logs.logMessage("Debug", "playlist.npRemove", "User requested a value to be removed but the nowPlaying track was not found in the library.", "system");
                        return false; // value was not found
                    }
                }
                else
                {
                    _logs.logMessage("Debug", "playlist.npRemove", "User requested a value to be removed but the nowPlaying var had no data.", "system");
                    return false; //npUrl had no value

                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.cmd_npRemove", error.ToString(), "system");
                return false;
            }
        }

        /// <summary>
        /// Used to display the currently queued up tracks by the users
        /// </summary>
        public string cmd_queue(int limit)
        {
            try
            {
                //check to see if we have anything queued up by the users
                string result = null;
                int counter = 0;

                if (listSubmitted.Count >= 1)
                {
                    //we have tracks submitted
                    for (int i = 0; i < listSubmitted.Count; i++)
                    {
                        if(counter == limit)
                        {
                            return result;
                        }
                        result = result + $"Title: {listSubmitted[i].title} - User: {listSubmitted[i].user} - Source: Submitted\r";
                        counter++;
                    }

                }

                for(int i = 0; i < listAutoQueue.Count; i++)
                {
                    if (counter != limit)
                    {
                        result = result + $"Title: {listAutoQueue[i].title} - User: {listAutoQueue[i].user} - Source: Library\r";
                        counter++;
                    }
                    else
                    {
                        return result;
                    }
                }
                return result;
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.cmd_queue", error.ToString(), "system");
                return null;
            }

        }

        public bool cmd_voteUp(string userID)
        {
            try
            {
                //figure out where the track is currently in the queue so we can update the record
                var result = listLibrary.FindIndex(x => x.url == npUrl);

                //get the infomation on likes in memory
                string[] likes = listLibrary[result].like;
                
                //figure out how many records we have already
                if (likes == null)
                {
                    likes = new string[] { userID };
                    listLibrary[result].like = likes;
                    savePlaylist();
                }
                else
                {
                    bool MatchFound = false;

                    //checking the current records to see if user is trying to vote twice
                    for (int i = 0; i < likes.Count(); i++)
                    {
                        if (likes[i] == userID)
                        {
                            //user already voted for the track
                            //breaking the loop
                            MatchFound = true;
                            i = likes.Count();
                        }
                    }

                    if (MatchFound == false) //if true, skips
                    {
                        //going to take the old records and append the new value 
                        List<string> temp = new List<string>();

                        // add the old values to the list
                        for (int i = 0; i < likes.Count(); i++)
                        {
                            temp.Add(likes[i]);
                        }

                        //add the new value
                        temp.Add(userID);

                        //take the new array and update the library
                        listLibrary[result].like = temp.ToArray();

                        //save the library
                        savePlaylist();
                    }
                }
                
                return true;
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "playlist.cmd_voteUp", error.ToString(), "system");
                return false;
            }
        }

        public int cmd_voteDown(string userID)
        {
            try
            {
                //figure out where the track is currently in the queue so we can update the record
                var result = listLibrary.FindIndex(x => x.url == npUrl);

                //get the infomation on likes in memory
                string[] skips = listLibrary[result].skips;

                //figure out how many records we have already
                if (skips == null)
                {
                    skips = new string[] { userID };
                    listLibrary[result].skips = skips;
                    savePlaylist();
                }
                else
                {
                    bool MatchFound = false;

                    if(skips.Count() == 2)
                    {
                        removeTrackSubmitted(npUrl);
                        return -1;
                    }

                    //checking the current records to see if user is trying to vote twice
                    for (int i = 0; i < skips.Count(); i++)
                    {
                        if (skips[i] == userID)
                        {
                            //user already voted for the track
                            //breaking the loop
                            MatchFound = true;
                            i = skips.Count();
                        }
                    }

                    if (MatchFound == false) //if true, skips
                    {
                        List<string> temp = new List<string>();

                        // add the old values to the list
                        for (int i = 0; i < skips.Count(); i++)
                        {
                            temp.Add(skips[i]);
                        }

                        //add the new value
                        temp.Add(userID);

                        //take the new array and update the library
                        listLibrary[result].skips = temp.ToArray();

                        //save the library
                        savePlaylist();
                    }
                }                
                return 1;
            }
            catch(Exception error)
            {

                return 0;
            }
        }

        public string cmd_searchLibrary(string mode, string query,string userName)
        {
            try
            {
                //read the start of the query and figure out what type of query they want
                string modeLower = mode.ToLower();
                string queryLower = query.ToLower(); //convert the query to lowercase to easy search


                List<ListPlaylist> tempList = new List<ListPlaylist>();
                switch (modeLower)
                {
                    case "title":
                        //take what was given and try to find a track for it
                        tempList = listLibrary.FindAll(x => x.title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
                        break;
                    default:
                       
                        break;
                }

                if(tempList.Count == 1)
                {
                    //queue up a track
                    return tempList[0].url;
                }
                else if(tempList.Count >= 2)
                {
                    //Display the results in chat to let the user refine the result
                    string returnResult = null;
                    for(int i = 0; i < tempList.Count; i++)
                    {
                        returnResult = returnResult + $"{i+1} Title: {tempList[i].title}\r";
                    }
                    
                    return returnResult;
                }
                else if(tempList.Count == 0)
                {
                    return $"Unable to find anything with the term {query}.";
                }

                return null;

            }
            catch(Exception error)
            {

                return null;
            }
        }

    }
}
