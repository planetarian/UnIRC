using System.Collections.Generic;

namespace UnIRC.Models
{
    public class Network
    {
        public string Name { get; set; }
        public List<Server> Servers { get; set; }
        public bool UseNetworkNick { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Nick { get; set; }
        public string BackupNick { get; set; }
    }
}
