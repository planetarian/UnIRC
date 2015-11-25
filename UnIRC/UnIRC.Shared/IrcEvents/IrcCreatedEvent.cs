namespace UnIRC.IrcEvents
{
    public class IrcCreatedEvent : IrcEvent
    {
        public string CreatedMessage { get; set; }

        public override string Output => CreatedMessage;

        public IrcCreatedEvent(IrcMessage m) : base(m)
        {
            CreatedMessage = m.Trailing;
        }
    }
}