using System.Collections.Generic;

namespace UnIRC.Models
{
    public class Network
    {
        public string Name { get; set; }
        public List<Server> Servers { get; set; }
    }
}
