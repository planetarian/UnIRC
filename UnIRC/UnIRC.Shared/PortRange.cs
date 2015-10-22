using System.Collections.Generic;

namespace UnIRC.Shared
{
    public class PortRange
    {
        public int StartPort { get; set; }
        public int EndPort { get; set; }
        public List<int> Ports { get; set; }
        public override string ToString()
        {
            return EndPort == StartPort
                ? StartPort.ToString()
                : $"{StartPort}-{EndPort}";
        }
    }
}
