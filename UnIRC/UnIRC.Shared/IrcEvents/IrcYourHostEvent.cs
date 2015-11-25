using System;
using System.Collections.Generic;
using System.Text;
using UnIRC.IrcEvents;

namespace UnIRC.Shared.IrcEvents
{

    public class IrcYourHostEvent : IrcEvent
    {
        public string Nick { get; set; }
        public string Host { get; set; }
        public string HostMessage { get; set; }

        public override string Output => HostMessage;

        public IrcYourHostEvent(IrcMessage m) : base(m)
        {
            Nick = m.Parameters[0];
            Host = m.Prefix;
            HostMessage = m.Trailing;
        }
    }
}
