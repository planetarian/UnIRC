namespace UnIRC.IrcEvents
{
    public abstract class IrcModeEvent : IrcEvent
    {
        public string Sender { get; protected set; }
        public string Modes { get; protected set; }

        protected IrcModeEvent(IrcMessage m) : base(m)
        {
            Sender = m.Prefix;
        }

        public static IrcModeEvent GetEvent(IrcMessage m)
        {
            if (m.Parameters.Length > 1 && m.Parameters[0].Substring(0, 1) == "#")
            {
                return new IrcChannelModeEvent(m);
            }
            return new IrcUserModeEvent(m);
        }
    }
}
