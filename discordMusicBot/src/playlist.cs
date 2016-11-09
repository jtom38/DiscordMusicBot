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

                    //going to add all the potential url's to this list
                    List<string> links = new List<string>();

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
                                        links.Add(line);
                                    }
                                }
                            }

                        }
                        catch(Exception e)
                        {

                        }                 
                    }

                    
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
        
        //todo thread this
        public string updatePlaylistFile()
        {
            string returnText = null;

            //this is slop atm I know.  I need to think of a better way to look for a url compared to a junk text
            if (_config.PlaylistURL.Contains("www."))
            {
                //we have a url
                if (File.Exists("autoplaylist2.txt"))
                {
                    //delete file?
                    File.Delete("autoplaylist2.txt");
                }

                using (WebClient download = new WebClient())
                {
                    string t = _config.PlaylistURL;
                    try
                    {
                        download.DownloadFile(_config.PlaylistURL, "autoplaylist2.txt");
                        returnText = "Updated the playlist.";
                        return returnText;
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
            }
            else
            {
                //we have something wrong with the url
                Console.WriteLine("Error: Failed to check the url in config.json.  Please check the string and try again.");
                returnText = "Error: Failed to check the url in config.json.  Please check the string and try again.";
                return returnText;
            }
        }

    }
}
