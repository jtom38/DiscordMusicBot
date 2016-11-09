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



    }
}
