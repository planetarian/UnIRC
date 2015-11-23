namespace UnIRC.IrcEvents
{
    public class IrcNamesEndEvent : IrcEvent
    {
        public string Channel { get; set; }
        public IrcNamesEndEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[1].ToLower();
        }
    }
}