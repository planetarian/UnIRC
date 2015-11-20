using System;
using System.Runtime.CompilerServices;

namespace UnIRC.Shared.Messages
{
    public class ReadyMessage : Message
    {
        public Type Subject { get; set; }

        // ReSharper disable ExplicitCallerInfoArgument
        public ReadyMessage(Type subject,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Ready", subject.Name + " is now ready for operation.",
                  callerFilePath, callerMemberName, callerLineNumber)
        {
            Subject = subject;
        }
    }
}
