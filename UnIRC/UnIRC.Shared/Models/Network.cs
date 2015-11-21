using System.Collections.Generic;
using UnIRC.Shared.Models;

namespace UnIRC.Models
{
    public class Network
    {
        public string Name { get; set; }
        public List<Server> Servers { get; set; }
        public bool UseNetworkNick { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
