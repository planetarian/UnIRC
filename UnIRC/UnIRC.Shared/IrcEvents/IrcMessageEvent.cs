using System;
using UnIRC.Models;

namespace UnIRC.IrcEvents
{
    public class IrcMessageEvent : IrcEvent
    {
        public string Sender { get; private set; }
        public string Target { get; private set; }
        public string Message { get; private set; }
        public bool IsChannelMessage { get; private set; }
        public bool IsServerMessage { get; private set; }
        public string ReturnTarget { get; private set; }

        public IrcMessageEvent(IrcMessage m) : base(m)
        {
            Sender = m.Prefix;
            Target = m.Parameters[0];
            Message = m.Trailing;
            IsChannelMessage = Target.Length > 1 && Target[0] == '#';
            IsServerMessage = String.IsNullOrWhiteSpace(Sender) || (!Sender.Contains("@") && Sender.Contains("."));
            
            if (!IsServerMessage)
                ReturnTarget = IsChannelMessage
                                   ? Target
                                   : new IrcUser(Sender, null, null).Nick;
        }
    }
}