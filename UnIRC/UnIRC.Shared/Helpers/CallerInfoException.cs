using System;
using System.Runtime.CompilerServices;

// ReSharper disable ExplicitCallerInfoArgument

namespace UnIRC.Shared.Helpers
{
    public class CallerInfoException : Exception
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string MemberName { get; set; }

        public CallerInfoException(
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1,
            [CallerMemberName] string memberName = "")
            : this(null, null, filePath, lineNumber, memberName)
        {
        }

        public CallerInfoException(
            string message,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1,
            [CallerMemberName] string memberName = "")
            : this(message, null, filePath, lineNumber, memberName)
        {
        }

        public CallerInfoException(
            string message = null,
            Exception innerException = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1,
            [CallerMemberName] string memberName = "")
            : base(message, innerException)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            MemberName = memberName;
        }


    }
}
