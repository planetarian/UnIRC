namespace UnIRC.IrcEvents
{
    public class IrcMotdBeginEvent : IrcEvent
    {
        public override string Output => $"- {IrcMessage.Prefix} Message of the day - ";

        public IrcMotdBeginEvent(IrcMessage m) : base(m)
        {
        }
    }
}