namespace UnIRC.IrcEvents
{
    public class IrcKickEvent : IrcChannelEvent
    {
        public string[] Kicked { get; private set; }
        public string Reason { get; private set; }

        public IrcKickEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[0].ToLower();
            Kicked = m.Parameters[1].Split(',');
            Reason = m.Trailing;
        }
    }
}