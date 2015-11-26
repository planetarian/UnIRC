namespace UnIRC.IrcEvents
{
    public class IrcNickInUseEvent : IrcEvent
    {
        public string OldNick { get; set; }
        public string NewNick { get; set; }

        public override string Output => $"{NewNick} Nickname is already in use.";

        public IrcNickInUseEvent(IrcMessage m) : base(m)
        {
            OldNick = m.Parameters[0];
            NewNick = m.Parameters[1];
        }
    }
}