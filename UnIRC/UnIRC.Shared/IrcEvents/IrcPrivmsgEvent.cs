namespace UnIRC.IrcEvents
{
    public class IrcPrivmsgEvent : IrcMessageEvent
    {
        public IrcPrivmsgEvent(IrcMessage m) : base(m)
        {
        }
    }
}