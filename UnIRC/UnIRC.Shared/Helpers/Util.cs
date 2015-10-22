using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Threading;

namespace UnIRC.Shared.Helpers
{
    public static class Util
    {
        public static DateTime Now => DateTime.Now;

        /// <summary>Disallowed characters for paths.</summary>
        public static List<char> BadPathCharacters => _badPathCharacters ??
                                                      (_badPathCharacters = Path.GetInvalidPathChars()
                                                          .Concat(new[] { '?', '*', ':' }).ToList());
        private static List<char> _badPathCharacters;

        /// <summary>
        /// Gets a version of the provided string which can safely be used as part of a file/folder path.
        /// </summary>
        /// <param name="name">System.String to convert.</param>
        /// <returns>The converted value.</returns>
        public static string GetSafeName(string name)
        {
            return BadPathCharacters.Aggregate(
#if DESKTOP
                HttpUtility
#else
                WebUtility
#endif
                    .HtmlDecode(name), (current, c) => current.Replace(c, '_'));
        }

        /// <summary>
        /// Turns the result of a BitConverter.ToString(byte[]) call back into a byte array.
        /// </summary>
        /// <param name="byteString">String of dash-separated bytes as returned by BitConverter.ToString(byte[])</param>
        /// <returns>Byte array representation of the provided byte string.</returns>
        public static byte[] ByteStringToBytes(string byteString)
        {
            int length = (byteString.Length + 1) / 3;
            var bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                char sixteen = byteString[3 * i];
                if (sixteen > '9') sixteen = (char)(sixteen - 'A' + 10);
                else sixteen -= '0';

                char ones = byteString[3 * i + 1];
                if (ones > '9') ones = (char)(ones - 'A' + 10);
                else ones -= '0';

                bytes[i] = (byte)(16 * sixteen + ones);
            }
            return bytes;
        }

        public static T KillerValue<T>(
            string name = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1,
            [CallerMemberName] string memberName = "")
        {
            throw new ValueNeededException(name, typeof(T), filePath, lineNumber, memberName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunOnUI(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }
    }
}
