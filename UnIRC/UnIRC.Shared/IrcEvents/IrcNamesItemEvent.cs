using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnIRC.Shared.Models;

namespace UnIRC.IrcEvents
{
    public class IrcNamesItemEvent : IrcEvent
    {
        public string Channel { get; private set; }
        public List<IrcUserNamesEntry> Entries { get; private set; }

        public override string Output => $"{Channel} {IrcMessage.Trailing}";

        private const string _namesRegex = @"^(?<prefix>[^a-zA-Z0-9-`^{}\[\]\\])?(?<nick>.+)$";

        public IrcNamesItemEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[2].ToLower();
            Entries = new List<IrcUserNamesEntry>();
            foreach (Match match in m.Trailing
                .Trim()
                .Split(' ')
                .Select(name => Regex.Match(name, _namesRegex, RegexOptions.Compiled)))
            {
                if (!match.Success)
                    throw new InvalidOperationException("Incorrectly-formatted NAMES response.");
                string nick = match.Groups["nick"].Value;
                string prefix = match.Groups["prefix"].Value;
                var entry = new IrcUserNamesEntry(nick, Channel, prefix);
                Entries.Add(entry);
            }
        }
    }
}