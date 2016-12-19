using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discordMusicBot.src
{
    class startup
    {
        private configuration _config;
        
        logs _logs = new logs();

        public void startupCheck()
        {            
            makeCacheFolder();
            makeCacheUploadedFolder();
            makeCacheCloudFolder();
            makeConfigFolder();
            checkConfigFile();
            checkToken();
            setOwnerID();
            setLogLevel();
            checkCommandPrefix();
        }

        private void makeCacheFolder()
        {
            try
            {
                if (Directory.Exists("cache"))
                {
                    return;
                }
                else
                {
                    Directory.CreateDirectory("cache");
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "program.makeCacheFolder", error.ToString(), "system");
            }
        }

        private void makeCacheUploadedFolder()
        {
            try
            {
                var t = Directory.Exists(Directory.GetCurrentDirectory() + "cache\\uploaded\\");

                if (t == false)
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\cache\\uploaded");
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "program.makeCacheUploadedFolder", error.ToString(), "system");
            }
        }

        private void makeCacheCloudFolder()
        {
            try
            {
                var t = Directory.Exists(Directory.GetCurrentDirectory() + "cache\\cloud\\");

                if (t == false)
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\cache\\cloud");
                }
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "program.makeCacheCloudFolder", error.ToString(), "system");
            }
        }

        private void makeConfigFolder()
        {
            if (Directory.Exists("configs"))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory("configs");
            }
        }

        private void checkConfigFile()
        {
            var configPath = Directory.GetCurrentDirectory() + "\\configs\\config.json";

            try
            {
                if (File.Exists(configPath))
                {
                    _config = configuration.LoadFile(configPath);
                }
                else
                {
                    _config = new configuration();
                    _config.SaveFile(configPath);
                }


            }
            catch
            {
                //unable to find the file
                _config = new configuration();
                _config.SaveFile(configPath);
            }
        }

        private void checkToken()
        {
            //check for the bot token
            try
            {
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                if (_config.Token != "")
                {
                    Console.WriteLine("Token has been found in config.json");
                }
                else
                {
                    Console.WriteLine("Please enter a valid token.");
                    Console.Write("Token: ");

                    _config.Token = Console.ReadLine();                     // Read the user's token from the console.
                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        private void setOwnerID()
        {
            try
            {
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                //ulong ownerID = _config.Owner;

                if (Int64.Parse(_config.Owner.ToString()) != 0)
                {
                    Console.WriteLine("Owner ID has been found in config.json");
                }
                else
                {
                    Console.WriteLine("Please enter your user ID to take ownership of this bot.");
                    Console.Write("ID: ");

                    ulong id = Convert.ToUInt64(Console.ReadLine());

                    _config.Owner = id;
                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error: {error}");
            }
        }

        private void checkCommandPrefix()
        {
            Console.WriteLine("Current commandPrefix = " + _config.Prefix);
        }

        private void setLogLevel()
        {
            try
            {
                _config = configuration.LoadFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");

                int t = _config.logLevel;
                if (_config.logLevel >= 0)
                {
                    switch (_config.logLevel)
                    {
                        case 0: //off
                            Console.WriteLine($"Logging: Off");
                            break;
                        case 1: //debug
                            Console.WriteLine($"Logging: Debug");
                            break;
                        case 2: //info
                            Console.WriteLine($"Logging: Infomation");
                            break;
                        case 3: //error
                            Console.WriteLine($"LOgging: Errors");
                            break;
                    }

                }
                else
                {
                    Console.WriteLine("Please enter what level of logging you would like.");
                    Console.WriteLine("0: Off");
                    Console.WriteLine("1: Debug");
                    Console.WriteLine("2: Infomation");
                    Console.WriteLine("3: Errors");
                    Console.Write("LogLevel: ");

                    int logLevel = 0;

                    int.TryParse(Console.ReadLine(), out logLevel);

                    _config.logLevel = logLevel;
                    _config.SaveFile(Directory.GetCurrentDirectory() + "\\configs\\config.json");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error: {error}");
            }
        }
    }
}
