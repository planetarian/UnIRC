using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcQuitEvent : IrcEvent
    {
        public IrcUser User { get; private set; }
        public string Reason { get; private set; }

        public override string Output => $"* Quits: {User.Nick} ({User.UserName}@{User.Host}) ({Reason})";

        public IrcQuitEvent(IrcMessage m) : base(m)
        {
            IrcUser user;
            IrcUser.TryGetUser(m.Prefix, out user);
            User = user;
            Reason = m.Trailing;
        }
    }
}