using System;
using System.Collections.Generic;

namespace UnIRC.Shared.Models
{
    public class Server
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public List<PortRange> Ports { get; set; }
        public string Password { get; set; }
        public override string ToString()
        {
            return $@"{Address}:{String.Join(",",Ports)}";
        }
    }
}
