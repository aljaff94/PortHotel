using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PortHotel
{
    public static class Ports
    {
        /// <summary>
        /// Get the first available port begin from the port seek
        /// </summary>
        /// <param name="startPort">From where looking for a free port</param>
        /// <param name="scanListeningOnly">Only current used port</param>
        /// <returns></returns>
        public static int PeekAvailablePort(int startPort = 1002, bool scanListeningOnly = false)
        {
            var listening = scanListeningOnly ? " -and $_.Contains(\"LISTENING\")" : null;
            using (var ps = PowerShell.Create())
            {
                while (true)
                {
                    ps.AddScript($"(netstat -an | where{{$_.Contains(\"{startPort}\"){listening}}}).count");
                    var result = (ps.Invoke()?[0]?.BaseObject) as int?;
                    if (result == null || result == 0)
                        return startPort;
                    startPort++;
                }
            }
        }

        /// <summary>
        /// Get the first available ip address begin from the ip seek
        /// </summary>
        /// <param name="startIp">From where looking for a free ip address</param>
        /// <returns></returns>
        public static string PeekAvailableIP(string startIp = "127.28.2.11")
        {
            var ipBytes = startIp.Split('.').Select<string, uint>(f => uint.Parse(f)).ToArray();
            using (var ps = PowerShell.Create())
            {
                while (true)
                {
                    ps.AddScript($"(netsh interface portproxy show v4tov4 | where{{$_.Contains(\"{startIp}\")}}).count");
                    var result = (ps.Invoke()?[0]?.BaseObject) as int?;
                    if (result == null || result == 0)
                        return startIp;

                    ipBytes[3]++;
                    if (ipBytes[3] >= 250)
                    {
                        ipBytes[3] = 10;
                        ipBytes[2]++;
                        if (ipBytes[2] >= 250)
                        {
                            ipBytes[2] = 11;
                            ipBytes[1]++;
                        }
                    }
                    startIp = String.Join(".", ipBytes);
                }
            }
        }

        /// <summary>
        /// Add a new port forwading entry
        /// </summary>
        /// <param name="listenAddress"></param>
        /// <param name="connectPort"></param>
        /// <param name="connectAddress"></param>
        public static void AddPortForward(string listenAddress, string listenPort, string connectPort, string connectAddress)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript($"netsh interface portproxy add v4tov4 listenport={listenPort} listenaddress={listenAddress} connectport={connectPort} connectaddress={connectAddress}");
                ps.Invoke();
            }
        }

        /// <summary>
        /// Delete a port forwarding entry
        /// </summary>
        /// <param name="listenAddress"></param>
        public static void DeletePortForward(string listenAddress, string listenPort)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript($"netsh interface portproxy delete v4tov4 listenport={listenPort} " +
                        $"listenaddress={listenAddress} ");
                ps.Invoke();
            }
        }

    }
}
