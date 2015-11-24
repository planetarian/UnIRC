namespace UnIRC.IrcEvents
{
    public class IrcPingEvent : IrcEvent
    {
        public string Content { get; set; }

        public IrcPingEvent(IrcMessage m) : base(m)
        {
            Content = m.Trailing;
        }
    }
}