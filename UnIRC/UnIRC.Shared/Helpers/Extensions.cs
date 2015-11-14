using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UnIRC.Shared.Helpers
{
    public static class Extensions
    {
        #region Expression

        public static string GetMemberName<T>(this Expression<T> expression)
        {
            Expression body = expression.Body;
            var memberExpression = body as MemberExpression;
            if (memberExpression != null)
                return memberExpression.Member.Name;

            var unaryExpression = body as UnaryExpression;
            return ((MemberExpression) unaryExpression?.Operand)?.Member.Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FunctionWrapper<T> WrapBoolean<T>
            (this T source, Expression<Func<T, bool>> expr, bool setTrue = true)
            where T : class
        {
            return new FunctionWrapper<T>(source, expr, setTrue);
        }

        #endregion Expression


        #region Object

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumber(this object value)
        {
            return value is int
                    || value is double
                    || value is decimal
                    || value is float
                    || value is long
                    || value is byte
                    || value is uint
                    || value is ulong
                    || value is sbyte
                    || value is short
                    || value is ushort;
        }

        #endregion Object


        #region String

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhitespace(this string str)
        {
            return String.IsNullOrWhiteSpace(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt32(this string value)
        {
            return Int32.Parse(value);
        }

        public static bool IsPositiveInteger(this string value)
        {
            ulong val;
            return UInt64.TryParse(value, out val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllNullOrEmpty(this IEnumerable<string> strings)
        {
            return strings.All(s => s.IsNullOrEmpty());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyNullOrEmpty(this IEnumerable<string> strings)
        {
            return strings.Any(s => s.IsNullOrEmpty());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllNullOrWhitespace(this IEnumerable<string> strings)
        {
            return strings.All(s => s.IsNullOrWhitespace());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyNullOrWhitespace(this IEnumerable<string> strings)
        {
            return strings.Any(s => s.IsNullOrWhitespace());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string str, char splitChar,
            StringSplitOptions options = StringSplitOptions.None)
        {
            return str.Split(new[] { splitChar }, options);
        }

        #endregion String


        #region DateTime

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(this DateTime value, DateTime start, DateTime end)
        {
            return value > start && value < end;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BetweenStartInclusive(this DateTime value, DateTime start, DateTime end)
        {
            return value >= start && value < end;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BetweenInclusive(this DateTime value, DateTime start, DateTime end)
        {
            return value >= start && value <= end;
        }

        public static TimeSpan PeriodIntersection(
            DateTime? a1, DateTime? a2, DateTime? b1, DateTime? b2)
        {
            DateTime max = DateTime.MaxValue;
            DateTime min = DateTime.MinValue;
            return new[] { a2 ?? max, b2 ?? max }.Min() -
                   new[] { a1 ?? min, b1 ?? min }.Max();
        }

        #endregion DateTime


        #region Numerics

        /// <summary>
        /// Returns an IEnumerable containing a sequential integer set,
        /// useful for iteration queries.
        /// </summary>
        /// <param name="start">First value in the result set.</param>
        /// <param name="end">Last value in the result set.</param>
        /// <returns>
        /// IEnumerable containing the set of all integers from
        /// <paramref name="start"/> to <paramref name="end"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="end"/> is less than <paramref name="start"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<int> To(this int start, int end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException(
                    nameof(end), $"{nameof(end)} must not be less than {nameof(start)}.");

            for (int i = start; i <= end; i++)
                yield return i;
        }

        #endregion


        #region IEnumerable

        /// <summary>
        /// Returns a copy of the provided IEnumerable with the specified item appended.
        /// Used for triggering UI updates on bound lists that do not implement notifications.
        /// </summary>
        /// <typeparam name="T">Type of the list contents and object to add.</typeparam>
        /// <param name="original">IEnumerable to add to.</param>
        /// <param name="item">Item to add.</param>
        /// <returns>New List based on the original IEnumerable and added item.</returns>
        public static IEnumerable<T> Plus<T>(this IEnumerable<T> original, T item)
        {
            yield return item;
            foreach (T i in original)
                yield return i;
            //var tempList = new List<T> { item };
            //tempList.AddRange(original);
            //return tempList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Minus<T>(this IEnumerable<T> original, T item)
        {
            return original.Where(i => !i.Equals(item));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        #endregion IEnumerable


        #region ICollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this ICollection coll)
        {
            return coll == null || coll.Count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSingle(this ICollection coll)
        {
            return coll != null && coll.Count == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasMultiple(this ICollection coll)
        {
            return coll != null && coll.Count > 1;
        }

        public static bool DataMatches<T, TKey>(
            this ICollection<T> first, ICollection<T> second,
            Func<T, TKey> selector)
        {
            if (first == null || second == null || first.Count != second.Count) return false;
            if (ReferenceEquals(first, second) || first.Count == 0) return true;
            IEnumerable<TKey> firstKeys = first.Select(selector);
            IEnumerable<TKey> secondKeys = second.Select(selector);
            bool equal = firstKeys.SequenceEqual(secondKeys);
            return equal;
        }

        #endregion ICollection

        
        #region Task

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ToBackground(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> ToBackground<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        #endregion Task


        #region ICommand

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute(this ICommand command)
        {
            command.Execute(null);
        }

        #endregion ICommand

    }
}
