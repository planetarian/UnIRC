using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcNoticeEvent : IrcMessageEvent
    {
        public override string Output => $"-{SourceUser?.Nick ?? Sender}- {Message}";

        public IrcNoticeEvent(IrcMessage m) : base(m)
        {
        }
    }
}