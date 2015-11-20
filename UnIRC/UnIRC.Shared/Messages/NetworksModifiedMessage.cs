using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UnIRC.Shared.Messages
{
    public class NetworksModifiedMessage : Message
    {
        // ReSharper disable ExplicitCallerInfoArgument
        public NetworksModifiedMessage(
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Networks modified", "User has modified network settings.",
                  callerFilePath, callerMemberName, callerLineNumber)
        {
            
        }
    }
}
