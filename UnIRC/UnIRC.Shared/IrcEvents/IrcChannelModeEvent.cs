namespace UnIRC.IrcEvents
{
    public class IrcChannelModeEvent : IrcModeEvent
    {
        public string Channel { get; private set; }
        public string ModeParameters { get; private set; }

        public IrcChannelModeEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[0];
            Modes = m.Parameters[1];

            string optional = m.Parameters.Length == 3 ? m.Parameters[2] : null;
            ModeParameters = optional;
        }
    }
}
