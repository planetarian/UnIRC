using System;

namespace UnIRC.IrcEvents
{
    public class IrcJoinEvent : IrcChannelEvent
    {
        public override string Output => $"* Joins: {User.Nick} ({User.UserName}@{User.Host})";

        public IrcJoinEvent(IrcMessage m) : base(m)
        {
            Channel = (m.Trailing == String.Empty ? m.Parameters[0] : m.Trailing).ToLower();
        }
    }
}