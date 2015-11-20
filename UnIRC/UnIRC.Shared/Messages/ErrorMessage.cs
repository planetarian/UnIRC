using System;
using System.Runtime.CompilerServices;

namespace UnIRC.Shared.Messages
{
    public class ErrorMessage : Message
    {
        public string Details { get; set; }
        public bool IsError { get; set; }
        public bool Display { get; set; }
        public Exception Exception { get; set; }

        // ReSharper disable ExplicitCallerInfoArgument
        public ErrorMessage(string description, string details = null,
            bool isError = true, bool display = true,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Error", description,
                  callerFilePath, callerMemberName, callerLineNumber)
        {
            Details = details;
            IsError = isError;
            Display = display;

        }

        public ErrorMessage(string description, Exception ex,
            bool isError = true, bool display = true,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = -1)
            : base("Error", description,
                  callerFilePath, callerMemberName, callerLineNumber)
        {
            Exception = ex;
            Details = ex?.Message;
            IsError = isError;
            Display = display;

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
