using System;
using System.Linq;

namespace UnIRC.IrcEvents
{
    public class IrcChannelModeEvent : IrcModeEvent
    {
        public string Channel { get; private set; }
        public string ModeParameters { get; private set; }

        public override string Output => $"* {User?.Nick??Sender} sets mode: {Modes} {ModeParameters}";

        public IrcChannelModeEvent(IrcMessage m) : base(m)
        {
            Channel = m.Parameters[0].ToLower();
            Modes = m.Parameters[1];

            ModeParameters = String.Join(" ", m.Parameters.Skip(2));
        }
    }
}
