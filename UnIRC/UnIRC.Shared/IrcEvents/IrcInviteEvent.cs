namespace UnIRC.IrcEvents
{
    public class IrcInviteEvent : IrcChannelEvent
    {
        public string Invitee { get; private set; }

        public override string Output => $"{Invitee} has been invited to {Channel} by {User.Nick}";

        public IrcInviteEvent(IrcMessage m) : base(m)
        {
            Invitee = m.Parameters[0];
            Channel = m.Trailing.ToLower();
        }

        
    }
}