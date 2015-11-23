using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcNoticeEvent : IrcMessageEvent
    {
        public IrcNoticeEvent(IrcMessage m) : base(m)
        {
        }
    }
}