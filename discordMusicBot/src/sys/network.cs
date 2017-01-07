using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace discordMusicBot.src.sys
{
    class network
    {
        logs _logs = new logs();

        public async Task<long> cmd_ping(string hostName)
        {
            try
            {
                await Task.Delay(1);

                Ping pingRequest = new Ping();

                var t = pingRequest.Send(hostName);

                return t.RoundtripTime;
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "system.cmd_ping", error.ToString(), "system");
                return -1;
            }
        }
    }
}
