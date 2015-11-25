using System;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.IrcEvents;

namespace UnIRC.IrcEvents
{
    public class IrcEvent
    {
        public string Type => GetType().Name;

        public DateTime Date { get; set; }
        public IrcMessage IrcMessage { get; set; }
        public IrcEventType EventType { get; set; }

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
            var m = new IrcMessage(rawData);//, prefix, command, parameters, trailing);

            switch (m.Command.ToLower())
            {
                case "user":
                    return new IrcUserEvent(m);
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
                case "ping":
                    return new IrcPingEvent(m);
                case "pong":
                    return new IrcPongEvent(m);
                case "001":
                    return new IrcWelcomeEvent(m);
                case "002":
                    return new IrcYourHostEvent(m);
                case "003":
                    return new IrcCreatedEvent(m);
                case "004":
                    return new IrcServerInfoEvent(m);
                case "352":
                    return new IrcWhoItemEvent(m);
                case "315":
                    return new IrcWhoEndEvent(m);
                case "353":
                    return new IrcNamesItemEvent(m);
                case "366":
                    return new IrcNamesEndEvent(m);
                case "372":
                    return new IrcMotdEvent(m);
                case "375":
                    return new IrcMotdBeginEvent(m);
                case "376":
                    return new IrcMotdEndEvent(m);
                case "error":
                    return new IrcErrorEvent(m);
                default:
                    return new IrcEvent(m);
            }
        }
        
        public override string ToString()
        {
            string output = IrcMessage.Command;
            if (!IrcMessage.ParameterString.IsNullOrEmpty())
                output += " " + IrcMessage.ParameterString;
            if (!IrcMessage.Trailing.IsNullOrEmpty())
                output += " :" + IrcMessage.Trailing;

            return $@"{Type}: {output}";
        }
    }
}