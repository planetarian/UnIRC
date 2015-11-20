using System;
using System.Runtime.CompilerServices;
using UnIRC.Shared.Helpers;

namespace UnIRC.Shared.Messages
{
    public class Message
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string FilePath { get; set; }
        public string MemberName { get; set; }
        public int LineNumber { get; set; }

        public Message(string name, string description,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
        {
            Date = Util.Now;

            Name = name;
            Description = description;

            FilePath = callerFilePath;
            MemberName = callerMemberName;
            LineNumber = callerLineNumber;
        }
    }
}
