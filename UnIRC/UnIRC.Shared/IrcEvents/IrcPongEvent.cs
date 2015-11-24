namespace UnIRC.IrcEvents
{
    public class IrcPongEvent : IrcEvent
    {
        public string Content { get; set; }

        public IrcPongEvent(IrcMessage m) : base(m)
        {
            Content = m.Trailing;
        }
    }
}