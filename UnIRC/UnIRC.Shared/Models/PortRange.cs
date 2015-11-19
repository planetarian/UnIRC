using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnIRC.Shared.Helpers;

namespace UnIRC.Models
{
    public class PortRange
    {
        public int StartPort { get; set; }
        public int EndPort { get; set; }

        [JsonIgnore]
        public IEnumerable<int> Ports => StartPort.To(EndPort);

        public PortRange()
        {
        }

        public PortRange(int port)
        {
            StartPort = port;
            EndPort = port;
        }

        public PortRange(int startPort, int endPort)
        {
            if (startPort > endPort)
                throw new ArgumentOutOfRangeException();

            StartPort = startPort;
            EndPort = endPort;
        }

        public override string ToString()
        {
            return EndPort == StartPort
                ? StartPort.ToString()
                : $"{StartPort}-{EndPort}";
        }
    }
}
