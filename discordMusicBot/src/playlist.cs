using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace discordMusicBot.src
{
    class playlist
    {
        public static List<string> listPlaylist = new List<string>();
        public static List<string> listQueue = new List<string>();
        public static List<string> listBlacklist = new List<string>();

        configuration _config = new configuration();

        public void getPlaylistFile()
        {
            // check for the local file
            if (File.Exists("autoplaylist.txt"))
            {
                //read the file
                using (StreamReader reader = new StreamReader("autoplaylist.txt"))
                {
                    //string we are going to fill per line
                    string line = null;

                    while((line = reader.ReadLine()) != null)
                    {
                        //look for the start of the string for #
                        try
                        {
                            //check for "" first
                            if(line != "")
                            {
                                //get the first character
                                string s = line.Substring(0, 1);
                                if(s != "#")
                                {
                                    if(line.Contains("https://www.youtube.com") ||
                                        line.Contains("http://www.youtube.com"))
                                    {
                                        //Console.WriteLine(line);
                                        listPlaylist.Add(line);
                                    }
                                }
                            }

                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Error in _playlist.getPlaylistFile.  Error: " + e);
                        }                 
                    }

                    //debug
                    int c = listPlaylist.Count();
                    
                    //we would want to pass this to the player with the list of links.

                }
            }
            else
            {
                Console.WriteLine("Unable to find autoplaylist.txt, generating one for you.");
                using (StreamWriter writer = new StreamWriter("autoplaylist.txt"))
                {
                    writer.Write("# the comment character is '#'");
                }

                
            }
        }

        public void getBlacklistFile()
        {
            if (File.Exists("blacklist.txt"))
            {

            }
        }

        public void autoPlayList()
        {

        } 
        
        //todo thread this
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

            using (FileStream fs = new FileStream("autoplaylist.txt", FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("# User:   " + user);
                sw.WriteLine("# Title:  " + title);
                sw.WriteLine(url);
            }
            return title;
        }

        public void cmd_shuffle()
        {

        }

    }
}
