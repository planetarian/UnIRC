using System;
using System.Collections.Generic;
using System.Text;
using UnIRC.IrcEvents;

namespace UnIRC.Shared.IrcEvents
{

    public class IrcUnknownCommandEvent : IrcEvent
    {
        public string Command { get; set; }

        public override string Output => $"Unknown command: {Command}";

        public IrcUnknownCommandEvent(IrcMessage m) : base(m)
        {
            Command = m.Command;
        }
    }
}
