using System;
using System.Runtime.CompilerServices;

namespace UnIRC.Shared.Helpers
{
    public class ValueNeededException : CallerInfoException
    {
        public string ValueName { get; set; }
        public Type ValueType { get; set; }

        public ValueNeededException(
            string valueName,
            Type valueType,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1,
            [CallerMemberName] string memberName = "")
            : base($"Value '{valueName}' of type {valueType} needs to be set.", filePath, lineNumber, memberName)
        {
            ValueName = valueName;
            ValueType = valueType;
        }
    }
}
