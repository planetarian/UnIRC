using System;
using System.Collections.Generic;
using System.Text;
using UnIRC.Models;

namespace UnIRC.Shared.Models
{
    public class IrcUserWhoEntry
    {
        public IrcUser User { get; private set; }
        public string Channel { get; private set; }
        public string Flags { get; private set; }

        public IrcUserWhoEntry(IrcUser user, string channel, string flags)
        {
            User = user;
            Channel = channel.ToLower();
            Flags = flags;
        }
    }
}
