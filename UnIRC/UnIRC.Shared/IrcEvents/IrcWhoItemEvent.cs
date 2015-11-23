using System;
using UnIRC.Models;
using UnIRC.Shared.Models;

namespace UnIRC.IrcEvents
{
    public class IrcWhoItemEvent : IrcEvent
    {
        public IrcUserWhoEntry Entry { get; set; }

        public IrcWhoItemEvent(IrcMessage m) : base(m)
        {
            string realName = m.Trailing.Substring(m.Trailing.IndexOf(' ') + 1);
            string[] p = m.Parameters;
            string channel = p[1].ToLower();
            var user = new IrcUser(p[5], p[2], p[3], realName, p[4]);
            var whoEntry = new IrcUserWhoEntry(user, channel, p[6]);
            Entry = whoEntry;
        }
    }
}