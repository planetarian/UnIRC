namespace UnIRC.IrcEvents
{
    public class IrcPongEvent : IrcEvent
    {
        public string Content { get; set; }

        public override string Output => $"PONG :{Content}";

        public IrcPongEvent(IrcMessage m) : base(m)
        {
            Content = m.Trailing;
        }
    }
}