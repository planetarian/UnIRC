using System;
using System.Collections.Generic;
using System.Text;

namespace UnIRC.Shared.Models
{
    public class IrcUserNamesEntry
    {
        public string Nick { get; private set; }
        public string Channel { get; private set; }
        public string Flags { get; private set; }

        public IrcUserNamesEntry(string nick, string channel, string flags)
        {
            Nick = nick;
            Channel = channel;
            Flags = flags;
        }
    }
}
