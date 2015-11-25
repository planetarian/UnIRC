namespace UnIRC.IrcEvents
{
    public class IrcPingEvent : IrcEvent
    {
        public string Content { get; set; }

        public override string Output => $"PING :{Content}";

        public IrcPingEvent(IrcMessage m) : base(m)
        {
            Content = m.Trailing;
        }
    }
}