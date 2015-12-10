namespace UnIRC.IrcEvents
{
    public class IrcPrivmsgEvent : IrcMessageEvent
    {
        public bool IsAction { get; private set; }

        public override string Output => IsAction
            ? $"* {SourceUser.Nick} {Message.Substring(8, Message.Length - 9)}"
            : $"<{SourceUser.Nick}> {Message}";

        public IrcPrivmsgEvent(IrcMessage m) : base(m)
        {
            IsAction = Message.StartsWith("\x0001ACTION ") && Message.EndsWith("\x0001");
        }
    }
}