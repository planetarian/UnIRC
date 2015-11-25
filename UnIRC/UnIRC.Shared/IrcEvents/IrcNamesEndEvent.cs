namespace UnIRC.IrcEvents
{
    public class IrcNamesEndEvent : IrcEvent
    {
        public string Channel { get; set; }

        public override string Output => $"{Channel} End of /NAMES list.";

        public IrcNamesEndEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[1].ToLower();
        }
    }
}