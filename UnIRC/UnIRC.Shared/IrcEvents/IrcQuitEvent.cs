using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcQuitEvent : IrcEvent
    {
        public IrcUser User { get; private set; }
        public string Reason { get; private set; }

        public IrcQuitEvent(IrcMessage m) : base(m)
        {
            User = new IrcUser(m.Prefix);
            Reason = m.Trailing;
        }
    }
}