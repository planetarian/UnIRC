namespace UnIRC.IrcEvents
{
    public class IrcWelcomeEvent : IrcEvent
    {
        public string Nick { get; private set; }

        public IrcWelcomeEvent(IrcMessage m) : base(m)
        {
            Nick = m.Parameters[0];
        }
    }
}
