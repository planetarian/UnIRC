﻿using System;

namespace UnIRC.IrcEvents
{
    public class IrcServerInfoEvent : IrcEvent
    {
        public string ServerName { get; set; }
        public string ServerVersion { get; set; }
        public string UserModes { get; set; }
        public string ChannelModes { get; set; }

        public override string Output => $"User modes: {UserModes} Channel modes: {ChannelModes}";

        public IrcServerInfoEvent(IrcMessage m) : base(m)
        {
            ServerName = m.Parameters[1];
            ServerVersion = m.Parameters[2];
            UserModes = m.Parameters[3];
            ChannelModes = m.Parameters[4];
        }
    }
}