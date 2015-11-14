using System.Collections.Generic;
using UnIRC.Shared.Helpers;

namespace UnIRC.Shared
{
    public class PortRange
    {
        public int StartPort { get; set; }
        public int EndPort { get; set; }

        public IEnumerable<int> Ports => StartPort.To(EndPort);

        public override string ToString()
        {
            return EndPort == StartPort
                ? StartPort.ToString()
                : $"{StartPort}-{EndPort}";
        }
    }
}
