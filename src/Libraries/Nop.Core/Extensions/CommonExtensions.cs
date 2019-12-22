using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Nop.Core.Extensions
{
    /// <summary>
    /// Common extensions
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Is null or default
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value to evaluate</param>
        /// <returns>Result</returns>
        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

        /// <summary>
        /// Get element value
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="elName">Eelement name</param>
        /// <returns>Value (text)</returns>
        public static string ElText(this XmlNode node, string elName)
        {
            return node?.SelectSingleNode(elName)?.InnerText;
        }
        public static bool IsIn<T>(this T obj, params T[] candidates)
        {
            return candidates.Any(c => c == null ? obj == null : c.Equals(obj));
        }

        public static bool IsIn<T>(this T obj, IEnumerable<T> candidates)
        {
            return obj.IsIn(candidates.ToArray());
        }
        public static bool IsNotIn<T>(this T obj, IEnumerable<T> candidates)
        {
            return !obj.IsIn(candidates.ToArray());
        }

        public static bool NotIn<T>(this T source, params T[] candidates)
        {
            return !candidates.Any(c => c == null ? source == null : c.Equals(source));
        }

        public static bool NotIn<T>(this T source, IEnumerable<T> candidates)
        {
            return source.NotIn(candidates.ToArray());
        }

        public static T Or<T>(this T source, T alternateIfSourceIsDefault)
        {
            return source.Equals(default(T)) ? alternateIfSourceIsDefault : source;
        }

        public static TOut IfPoss<T, TOut>(this T? nullable, Func<T, TOut> getter, TOut valueIfNotPoss = default(TOut)) where T : struct
        {
            return nullable.Cond(t => !t.HasValue, t => valueIfNotPoss, t => getter(t.Value));
        }

        public static TOut IfPoss<T, TOut>(this T obj, Func<T, TOut> getter, TOut valueIfNotPoss = default(TOut))
            where T : class
        {
            return obj.Cond(t => t == null, t => valueIfNotPoss, getter);
        }

        public static string IfNotNullOrEmpty(this string obj, Func<string, string> getter, string valueIfNotPoss = null)
        {
            return obj.IsNotNullOrEmpty() ? getter(obj) : valueIfNotPoss;
        }

        public static T Modify<T>(this T obj, Action<T> modifier)
        {
            modifier(obj);
            return obj;
        }

        public static TOut Use<T, TOut>(this T obj, Func<T, TOut> usage)
        {
            return usage(obj);
        }

        public static TOut Cond<T, TOut>(this T obj, Func<T, bool> test, Func<T, TOut> resultIf, Func<T, TOut> resultElse)
        {
            return test(obj) ? resultIf(obj) : resultElse(obj);
        }

        public static T? AsNullable<T>(this T t) where T : struct
        {
            return t;
        }

        public static T ToEnum<T>(this int i) where T : struct, IConvertible
        {
            var @enum = (T)Enum.Parse(typeof(T), i.ToString());
            return @enum;
        }

        public static bool InRange(this int i, int bottom, int top, bool inclusive = true)
        {
            return inclusive
                ? bottom <= i && i <= top
                : bottom < i && i < top;
        }

        public static int ToInt<T>(this T e) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            return (int)(IConvertible)e;
        }

        public static double ToDouble(this decimal value)
        {
            return Convert.ToDouble(value);
        }
    }

}
