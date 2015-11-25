namespace UnIRC.IrcEvents
{
    public class IrcMotdEvent : IrcEvent
    {
        public string Message { get; set; }

        public override string Output => Message;

        public IrcMotdEvent(IrcMessage m) : base(m)
        {
            Message = m.Trailing;
        }
    }
}