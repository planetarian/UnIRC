namespace UnIRC.IrcEvents
{
    public class IrcWelcomeEvent : IrcEvent
    {
        public string Nick { get; private set; }
        public string Message { get; private set; }

        public override string Output => Message;

        public IrcWelcomeEvent(IrcMessage m) : base(m)
        {
            Nick = m.Parameters[0];
            Message = m.Trailing;
        }
    }
}
