﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnIRC.Models;
using UnIRC.Shared.Models;

namespace UnIRC.Shared.Messages
{
    public class ConnectMessage : Message
    {
        public Network Network { get; set; }
        public Server Server { get; set; }
        public UserInfo UserInfo { get; set; }

        // ReSharper disable ExplicitCallerInfoArgument
        public ConnectMessage(Network network, Server server, UserInfo userInfo,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Connect", $@"User initiated connection to '{network?.Name}' via {server?.DisplayName}",
                  callerFilePath, callerMemberName, callerLineNumber)
        {
            Network = network;
            Server = server;
            UserInfo = userInfo;
        }
    }
}
