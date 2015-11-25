namespace UnIRC.IrcEvents
{
    public class IrcMotdEndEvent : IrcEvent
    {
        public override string Output => "End of /MOTD command.";

        public IrcMotdEndEvent(IrcMessage m) : base(m)
        {
        }
    }
}