namespace UnIRC.IrcEvents
{
    public class IrcInviteEvent : IrcChannelEvent
    {
        public string Invitee { get; private set; }

        public IrcInviteEvent(IrcMessage m) : base(m)
        {
            Invitee = m.Parameters[0];
            Channel = m.Trailing.ToLower();
        }
    }
}