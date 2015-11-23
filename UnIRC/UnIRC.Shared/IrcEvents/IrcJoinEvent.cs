namespace UnIRC.IrcEvents
{
    public class IrcJoinEvent : IrcChannelEvent
    {

        public IrcJoinEvent(IrcMessage m) : base(m)
        {
            Channel = m.Trailing;
        }
    }
}