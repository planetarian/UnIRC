using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnIRC.Shared.Helpers;

namespace UnIRC.Models
{
    public class Server
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public List<PortRange> Ports { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
        public bool UseServerNick { get; set; }
        public string Nick { get; set; }
        public string BackupNick { get; set; }

        [JsonIgnore]
        public string DisplayName => Name.IsNullOrWhitespace() ? ToString() : Name;

        public override string ToString()
        {
            return GetServerDisplayName(Address, Ports, UseSsl);
        }

        public static string GetServerDisplayName(string address, IEnumerable<PortRange> ports, bool useSsl)
        {
            return $@"{(useSsl ? "+" : "")}{address}:{String.Join(",", ports)}";
        }
    }
}
