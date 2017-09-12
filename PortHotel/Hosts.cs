using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortHotel
{
    public static class Hosts
    {
        static readonly string _hostPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            (Environment.OSVersion.Platform == PlatformID.Win32NT ? "system32\\drivers\\etc\\hosts" : "hosts"));

        /// <summary>
        /// Add a new enty to the windows hosts file
        /// </summary>
        /// <param name="ipAddress">The mapping ip address to host name</param>
        /// <param name="hostName">The hostname that would be mapping</param>
        /// <param name="comment">If you've a comment put it or leave it blank</param>
        public static void AddEntry(string ipAddress, string dns, string comment = null)
        {
            var lines = new List<string>(File.ReadAllLines(_hostPath));
            lines.RemoveAll(f => f.Contains(dns));

            lines.Add($"{ipAddress} {dns} {(!String.IsNullOrWhiteSpace(comment) ? $"\t{comment}" : "")}");
            File.WriteAllLines(_hostPath, lines);
        }

        /// <summary>
        /// Remove an entry from windows hosts file.
        /// </summary>
        /// <param name="hostName">The hostname that you would to remove</param>
        public static void RemoveEntry(string dns)
        {
            var lines = new List<string>(File.ReadAllLines(_hostPath));
            lines.RemoveAll(f => f.Contains(dns));

            File.WriteAllLines(_hostPath, lines);
        }
    }
}
