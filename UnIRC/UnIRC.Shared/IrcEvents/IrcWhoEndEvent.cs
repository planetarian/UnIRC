using System;

namespace UnIRC.IrcEvents
{
    public class IrcWhoEndEvent : IrcEvent
    {
        public string Channel { get; private set; }

        public IrcWhoEndEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[1].ToLower();
        }
    }
}