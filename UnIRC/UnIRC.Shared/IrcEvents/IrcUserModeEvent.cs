﻿namespace UnIRC.IrcEvents
{
    public class IrcUserModeEvent : IrcModeEvent
    {
        public string Target { get; private set; }

        public override string Output => $"* {Sender} sets mode: {Modes}";

        public IrcUserModeEvent(IrcMessage m) : base(m)
        {
            Target = m.Parameters[0];
            Modes = m.Trailing;
        }
    }
}