using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UnIRC.Shared.Helpers
{
    public static class RandomHelper
    {
        private static readonly Random _randomSeed = new Random();

        public static string RandomSentence(int minWords, int maxWords, int minWordLength, int maxWordLength)
        {
            int words = RandomInt(minWords, maxWords);
            var sb = new StringBuilder();
            for (int i = 1; i <= words; i++)
            {
                int wordLength = RandomInt(minWordLength, maxWordLength);
                if (i == 1)
                {
                    wordLength--;
                    sb.Append(RandomString(1));
                }
                sb.Append(RandomString(wordLength, true));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
        public static string RandomString(int size, bool lowerCase = false)
        {
            // StringBuilder is faster than using strings (+=)
            var randStr = new StringBuilder(size);

            // Ascii start position (65 = A / 97 = a)
            var start = (lowerCase) ? 97 : 65;

            // Add random chars
            for (var i = 0; i < size; i++)
                randStr.Append((char)(26 * _randomSeed.NextDouble() + start));

            return randStr.ToString();
        }

        public static int RandomInt(int min, int max)
        {
            return _randomSeed.Next(min, max);
        }

        public static long RandomLong(long min, long max)
        {
            return (long)(min + RandomDouble() * (max - min));
        }

        public static double RandomDouble()
        {
            return _randomSeed.NextDouble();
        }

        public static double RandomDouble(double min, double max)
        {
            return min + RandomDouble() * (max - min);
        }

        public static double RandomNumber(int min, int max, int digits)
        {
            return Math.Round(_randomSeed.Next(min, max - 1) + _randomSeed.NextDouble(), digits);
        }

        public static bool RandomBool()
        {
            return (_randomSeed.NextDouble() > 0.5);
        }

        public static DateTime RandomDate()
        {
            return RandomDate(new DateTime(1900, 1, 1), Util.Now);
        }

        public static DateTime RandomDate(DateTime from, DateTime to)
        {
            var range = new TimeSpan(to.Ticks - from.Ticks);
            return from + new TimeSpan((long)(range.Ticks * _randomSeed.NextDouble()));
        }
        
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static double ScaleValue(double value, double valueScale, double targetScale)
        {
            return value != 0 && valueScale != 0
                ? targetScale/valueScale*value
                : 0;
        }
    }
}
