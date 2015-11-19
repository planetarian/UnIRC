using System;
using System.Collections.Generic;
using System.Text;

namespace UnIRC.Shared.Messages
{
    public class NetworksModifiedMessage : Message
    {
        public NetworksModifiedMessage()
            : base("Networks modified", "User has modified network settings.")
        {
            
        }
    }
}
