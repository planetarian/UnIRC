using System;
using System.Collections.Generic;
using UnIRC.Shared.Helpers;

namespace UnIRC.Shared.Models
{
    public class PortRange
    {
        public int StartPort { get; set; }
        public int EndPort { get; set; }

        public IEnumerable<int> Ports => StartPort.To(EndPort);

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
