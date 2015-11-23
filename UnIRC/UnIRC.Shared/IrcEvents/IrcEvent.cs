using System;
using System.Text.RegularExpressions;

namespace UnIRC.IrcEvents
{
    public class IrcEvent
    {
        public string Type => GetType().Name;

        public DateTime Date { get; set; }
        public IrcMessage IrcMessage { get; set; }

        private const string _messageRegexPattern =
            "^(:(?<prefix>[^ ]+) +)?(?<command>[^ ]+)(?<innerparams>( +[^ ]+)*?)?( +:(?<outerparams>.*))?$";

        public string FormattedDate => $"[{Date:T}] ";
        public virtual string Output => $"{ToString()}";
        public string TimestampedOutput => $"{FormattedDate}{Output}";


        public IrcEvent(IrcMessage ircMessage)
        {
            Date = DateTime.Now;
            IrcMessage = ircMessage;
        }

        public static IrcEvent GetEvent(string rawData)
        {
            Match match = Regex.Match(rawData, _messageRegexPattern, RegexOptions.Compiled);
            if (!match.Success)
                throw new InvalidOperationException("IRCMessage data invalid.");

            string prefix = match.Groups["prefix"].Value;
            string command = match.Groups["command"].Value;
            string[] parameters = match.Groups["innerparams"].Value
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string trailing = match.Groups["outerparams"].Value;
            var m = new IrcMessage(rawData, prefix, command, parameters, trailing);

            switch (command.ToLower())
            {
                case "mode":
                    return IrcModeEvent.GetEvent(m);
                case "privmsg":
                    return new IrcPrivmsgEvent(m);
                case "notice":
                    return new IrcNoticeEvent(m);
                case "nick":
                    return new IrcNickEvent(m);
                case "join":
                    return new IrcJoinEvent(m);
                case "part":
                    return new IrcPartEvent(m);
                case "kick":
                    return new IrcKickEvent(m);
                case "quit":
                    return new IrcQuitEvent(m);
                case "invite":
                    return new IrcInviteEvent(m);
                case "352":
                    return new IrcWhoItemEvent(m);
                case "315":
                    return new IrcWhoEndEvent(m);
                case "353":
                    return new IrcNamesItemEvent(m);
                case "366":
                    return new IrcNamesEndEvent(m);
                default:
                    return new IrcEvent(m);
            }
        }


        public override string ToString()
        {
            return $@"{Type}: {IrcMessage.RawMessage}";
        }
    }

    public class IrcPingEvent : IrcEvent
    {
        public string Content { get; set; }

        public IrcPingEvent(IrcMessage m) : base(m)
        {
            Content = m.Trailing;
        }
    }
}