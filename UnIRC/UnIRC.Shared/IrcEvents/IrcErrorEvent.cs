namespace UnIRC.IrcEvents
{
    public class IrcErrorEvent : IrcEvent
    {
        public string Message { get; set; }

        public override string Output => $"[ ERROR: {Message} ]";

        public IrcErrorEvent(IrcMessage m) : base(m)
        {
            Message = m.Trailing;
        }

        public IrcErrorEvent(string message)
            : this(new IrcMessage("ERROR") {Trailing = message})
        {
        }
    }
}