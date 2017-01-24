﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace discordMusicBot.src.sys
{
    class logs
    {
        public configuration _config;

        public async Task logMessageAsync(string level, string source, string msg, string user)
        {
            try
            {

                //check what the user wants returned
                bool configLevel = await returnConfigLogLevelAsync(level);

                if (configLevel == true)
                {
                    bool fileCheck = await checkLogSizeAsync();
                    if (configLevel == true)
                    {
                        await logFileAsync(level, source, msg, user);
                    }
                }
            }
            catch
            {

            }
        }

        private async Task<bool> returnConfigLogLevelAsync(string level)
        {
            try
            {
                await Task.Delay(1);

                _config = configuration.LoadFile();

                switch (_config.logLevel)
                {
                    case 0: // off
                        break;
                    case 1: // debug catch everything
                        if (level == "Debug" ||
                            level == "Info" ||
                            level == "Error")
                            return true;
                        break;
                    case 2: // info and errors
                        if (level == "Info" ||
                            level == "Error")
                            return true;
                        break;
                    case 3: // errors only
                        if (level == "Error")
                            return true;
                        break;
                }
                return false;
            }
            catch
            {

                return false;
            }

        }

        private async Task<bool> logFileAsync(string level, string source, string msg, string user)
        {
            try
            {
                await checkLogSizeAsync();

                //if we dont find the logs.txt make it
                if (!File.Exists("logs.txt"))
                    File.Create("logs.txt");

                //write our event to the logs.txt file
                using (StreamWriter txtLog = new StreamWriter("logs.txt", true))
                {
                    txtLog.WriteLine($"[{level}] {msg} - {source} - {user} - {DateTime.Now}");
                }

                //also write to the console so the admin can see it and I can when debugging.
                Console.WriteLine($"[{level}] {msg} - {source} - {user} - {DateTime.Now}");

                return true;
            }
            catch(Exception error)
            {
                Console.WriteLine(error);
                return false;
            }

        }

        private async Task<bool> checkLogSizeAsync()
        {
            try
            {
                await Task.Delay(1);

                string filePath = Directory.GetCurrentDirectory() + "\\logs.txt";

                long length = new System.IO.FileInfo(filePath).Length;

                if(length >= 1048576)
                {
                    //the file is now 1MB, archive it and make a fresh file.

                    File.Move("logs.txt", $"logs_archive{DateTime.Now.ToString()}");

                    await logMessageAsync("Info", "logs.checkLogSize", "The log file was 1mb, archiving it and making a fresh file.", "system");
                }

                return true;

            }
            catch(Exception error)
            {
                Console.WriteLine(error);
                return false;
            }
        }
        
        public void logMessage(string level, string source, string msg, string user)
        {
            try
            {
                //check what the user wants returned
                bool configLevel = returnConfigLogLevel(level);

                if(configLevel == true)
                    logFile(level, source, msg, user);
            }
            catch
            {
                //Console.WriteLine(error.ToString());
            }
        }

        private bool returnConfigLogLevel(string level)
        {
            try
            {
                _config = configuration.LoadFile();

                switch (_config.logLevel)
                {
                    case 0: // off
                        break;
                    case 1: // debug catch everything
                        if (level == "Debug" ||
                            level == "Info" ||
                            level == "Error")
                            return true;
                        break;
                    case 2: // info and errors
                        if (level == "Info" ||
                            level == "Error")
                            return true;
                        break;
                    case 3: // errors only
                        if (level == "Error")
                            return true;
                        break;
                }
                return false;
            }
            catch
            {

                return false;
            }

        }

        private bool logFile(string level, string source, string msg, string user)
        {
            try
            {
                checkLogSize();

                //if we dont find the logs.txt make it
                if (!File.Exists("logs.txt"))
                    File.Create("logs.txt");

                //write our event to the logs.txt file
                using (StreamWriter txtLog = new StreamWriter("logs.txt", true))
                {
                    txtLog.WriteLine($"[{level}] {msg} - {source} - {user} - {DateTime.Now}");
                }

                //also write to the console so the admin can see it and I can when debugging.
                Console.WriteLine($"[{level}] {msg} - {source} - {user} - {DateTime.Now}");

                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return false;
            }

        }

        private bool checkLogSize()
        {
            try
            {

                string filePath = Directory.GetCurrentDirectory() + "\\logs.txt";

                long length = new System.IO.FileInfo(filePath).Length;

                if (length >= 1048576)
                {
                    //the file is now 1MB, archive it and make a fresh file.

                    File.Move("logs.txt", $"logs_archive{DateTime.Now.ToString()}");

                    logMessage("Info", "logs.checkLogSize", "The log file was 1mb, archiving it and making a fresh file.", "system");
                }

                return true;

            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return false;
            }
        }
    }
}
