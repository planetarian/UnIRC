namespace UnIRC.IrcEvents
{
    public class IrcPrivmsgEvent : IrcMessageEvent
    {
        public override string Output => $"<{SourceUser.Nick}> {Message}";

        public IrcPrivmsgEvent(IrcMessage m) : base(m)
        {
        }
    }
}