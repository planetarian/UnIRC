using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcChannelEvent : IrcEvent
    {
        public IrcUser User { get; set; }
        public string Channel { get; set; }

        public IrcChannelEvent(IrcMessage m) : base(m)
        {
            IrcUser user;
            IrcUser.TryGetUser(m.Prefix, out user);
            User = user;
        }
    }
}