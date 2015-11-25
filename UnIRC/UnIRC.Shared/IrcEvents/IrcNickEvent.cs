using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcNickEvent : IrcEvent
    {
        public IrcUser User { get; private set; }
        public string OldNick { get; private set; }
        public string NewNick { get; private set; }

        public override string Output => $"* {OldNick} is now known as {NewNick}";

        public IrcNickEvent(IrcMessage m) : base(m)
        {
            IrcUser user;
            IrcUser.TryGetUser(m.Prefix, out user);
            User = user;

            OldNick = User.Nick;
            NewNick = m.Parameters.Length > 0 ? m.Parameters[0] : m.Trailing;
        }
    }
}