using System;
using System.Runtime.CompilerServices;

namespace UnIRC.Shared.Messages
{
    public class ErrorMessage : Message
    {
        public string Details { get; private set; }
        public bool IsError { get; set; }
        public bool Display { get; set; }
        public Exception Exception { get; private set; }
        public string FilePath { get; private set; }
        public string MemberName { get; private set; }
        public int LineNumber { get; private set; }

        public ErrorMessage(string description, string details = null,
            bool isError = true, bool display = true,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Error", description)
        {
            Details = details;
            IsError = isError;
            Display = display;

            FilePath = callerFilePath;
            MemberName = callerMemberName;
            LineNumber = callerLineNumber;
        }

        public ErrorMessage(string description, Exception ex,
            bool isError = true, bool display = true,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Error", description)
        {
            Exception = ex;
            Details = ex?.Message;
            IsError = isError;
            Display = display;

            FilePath = callerFilePath;
            MemberName = callerMemberName;
            LineNumber = callerLineNumber;

            Exception inner = ex?.InnerException;
            if (inner != null)
                Details += Environment.NewLine; // extra line
            while (inner != null)
            {
                Details += Environment.NewLine + "Inner Exception: " + inner.Message;
                inner = inner.InnerException;
            }
        }
    }
}
