namespace UnIRC.IrcEvents
{
    public class IrcPartEvent : IrcChannelEvent
    {
        public override string Output => $"* Parts: {User.Nick} ({User.UserName}@{User.Host})";

        public string Reason { get; private set; }

        public IrcPartEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[0].ToLower();
            Reason = m.Trailing;
        }
    }
}