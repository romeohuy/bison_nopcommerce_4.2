using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Nop.Core.Extensions
{
    public static class CharExtensions
    {
        public static int? ToIntOrNull(this char c)
        {
            return c.ToString().ToIntOrNull();
        }
    }

    public static class StringExtensions
    {
        public static decimal ToDecimalOrDefault(this string value, decimal defaultValue = 0)
        {
            decimal n;
            return decimal.TryParse(value, out n) ? n : defaultValue;
        }

        public static decimal? ToDecimalOrNull(this string value)
        {
            var n = value.ToDecimalOrDefault();
            return n > 0 ? (decimal?)n : null;
        }

        public static double ToDoubleOrDefault(this string value, double defaultValue = 0)
        {
            double n;
            return double.TryParse(value, out n) ? n : defaultValue;
        }

        public static int ToIntOrDefault(this string s, int defaultValue = -1)
        {
            int result;
            return int.TryParse(s, out result) ? result : defaultValue;
        }

        public static int ToIntOrMinus1(this string s)
        {
            int result;
            return int.TryParse(s, out result) ? result : -1;
        }

        public static int ToIntODefault(this string s, int @default = 0)
        {
            int result;
            return int.TryParse(s, out result) ? result : @default;
        }

        public static long ToLongODefault(this string s, long @default = 0)
        {
            long result;
            return long.TryParse(s, out result) ? result : @default;
        }

        public static int ToPossitiveIntOrDefault(this string s, int @default = 0)
        {
            var result = s.ToIntOrDefault(@default);
            return result > 0 ? result : @default;
        }

        public static int? ToIntOrNull(this string s)
        {
            int result;
            return int.TryParse(s, out result) ? result : (int?)null;
        }

        public static string Normalise(this string text)
        {
            return RemoveDiacritics(text).Trim().ToLowerInvariant();
        }

        public static string RemoveDiacritics(this string text)
        {
            //http://stackoverflow.com/a/249126/3856
            //note this only removes diacritics it doesn't ascii-ise
            //characters that are genuine single characters (e.g. ß, æ)
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ToPascalCase(this string s, char delimiter = ' ', bool keepDelimiter = true)
        {
            if (s == null) return null;
            if (s.Length < 2) return s.ToUpper();

            var words = s.Split(delimiter);

            var result = string.Join(keepDelimiter ? delimiter.ToString() : string.Empty, words.Select(word => word.FirstLetterToUpper()).ToArray());
            return result;
        }

        public static string GetUrlPath(this string s)
        {
            s = s.Trim();

            if (s.StartsWith("http"))
            {
                return new Uri(s).AbsolutePath;
            }

            s = s.StartsWith("/") ? s : "/{0}".FormatWith(s);
            var index = s.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
            return s.Substring(0, index == -1 ? s.Length : index).ToLowerInvariant();
        }

        public static string TrimTo(this string s, int count)
        {
            return s == null ? string.Empty : s.Length > count ? s.Substring(0, count) + "..." : s;
        }

        public static string FirstXAsciiCharacters(this string s, int count)
        {
            return new string(s.ToLowerInvariant().Where(IsAsciiAz).Take(count).Select(s1 => s1).ToArray());
        }

        public static bool InvariantCultureIgnoreCaseCompare(this string obj, string compareTo)
        {
            return string.Equals(obj, compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static T ChangeType<T>(this string obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static string StripLineBreaks(this string s, string replace = "")
        {
            return s.Replace("\r", replace).Replace("\n", replace).Trim();
        }

        public static string GetQueueName(this string destination)
        {
            var parts = destination.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 1];
        }

        public static Dictionary<string, T> ToDictionary<T>(this string s, Func<string, T> valueAction)
        {
            if (s.IsNullOrEmpty())
                return new Dictionary<string, T>();

            return s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(entry => entry.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(kvp => kvp[0], kvp => valueAction(kvp.Length > 1 ? kvp[1] : null));
        }

        public static string GetNumbers(this string s, int? count = null)
        {
            var result = s.AsEnumerable().Where(char.IsDigit);
            return new String(count.HasValue && count.Value < result.Count() ? result.Take(count.Value).ToArray() : result.ToArray());
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNotNullOrEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        public static string RavenEscape(this string s)
        {
            return s.Replace(":", "");
        }

        public static string FormatWith(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static string TryFormatWith(this string s, string failureValue, params object[] args)
        {
            try
            {
                return string.Format(s, args);
            }
            catch
            {
                return failureValue;
            }
        }

        public static string LastElement(this string input, char seperator)
        {
            if (input == null)
                return string.Empty;

            string[] elements = input.Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
            return elements[elements.Length - 1];
        }

        public static string Base64Encode(this string input)
        {
            if (input.IsNullOrEmpty())
                return string.Empty;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static string Base64Decode(this string input)
        {
            if (input.IsNullOrEmpty())
                return string.Empty;

            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }

        public static string ReplaceFirstOccurrance(this string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;

            int loc = original.IndexOf(oldValue, StringComparison.InvariantCultureIgnoreCase);
            return loc >= 0 ? original.Remove(loc, oldValue.Length).Insert(loc, newValue) : original;
        }

        public static string ReplaceLastOccurrance(this string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;

            int loc = original.LastIndexOf(oldValue, StringComparison.InvariantCultureIgnoreCase);
            return loc >= 0 ? original.Remove(loc, oldValue.Length).Insert(loc, newValue) : original;
        }

        public static string Hash(this string input)
        {
            if (input.IsNullOrEmpty()) return string.Empty;

            // step 1, calculate MD5 hash from input
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                var sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static string Quantity(this string singularForm, int qty, string irregularPluralForm = null)
        {
            return qty + " " + Pluralise(singularForm, qty, irregularPluralForm);
        }

        public static string Pluralise(this string singularForm, int qty, string irregularPluralForm = null)
        {
            if (qty == 1)
                return singularForm;

            return irregularPluralForm ?? (singularForm + "s");
        }

        /// <summary>
        /// Remove all characters except 0-9 and a-z, replace spaces with - and remove double spaces at the end
        /// </summary>
        /// <param name="value"></param>
        /// <param name="spaceReplacement"></param>
        /// <param name="allowPeriod"></param>
        /// <returns></returns>
        public static string SanitizeForUrl(this string value, string spaceReplacement = "-", bool allowPeriod = false)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var builder = new StringBuilder();

            char[] originalChars = value.UnicodeToAscii().Trim().ToLowerInvariant().ToCharArray();

            foreach (char t in originalChars)
            {
                if (AcceptableUrlChar(t)) //only allow a-z and 0-9
                    builder.Append(t);
                else if (t == ' ') //replace spaces with hyphen
                    builder.Append(spaceReplacement);
                else if (t == '.' && allowPeriod)
                    builder.Append(".");
            }

            return builder.ToString().Replace("--", "-");
        }

        public static string UnicodeToAscii(this string text)
        {
            //http://stackoverflow.com/a/2086575/3856
            //note that ß -> ? with this
            byte[] tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
            string asciiStr = Encoding.UTF8.GetString(tempBytes);
            return asciiStr;
        }

        /// <summary>
        /// Only returns true if character is a-z or 0-9
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private static bool AcceptableUrlChar(char character)
        {
            return ((character > 96 && character < 123) || (character > 47 && character < 58) || character == '-' || (character > 191 && character < 383));
        }

        /// <summary>
        /// Only returns true if character is a-z
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private static bool IsAsciiAz(char character)
        {
            return (character > 96 && character < 123);
        }

        /// <summary>
        /// Remove illegal XML characters from a string. For more info see this http://stackoverflow.com/a/12469826/52360
        /// </summary>
        public static string SanitizeForXml(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var buffer = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (IsLegalXmlChar(c))
                {
                    buffer.Append(c);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        public static bool IsLegalXmlChar(int character)
        {
            return
            (
                character == 0x9 /* == '\t' == 9   */ ||
                character == 0xA /* == '\n' == 10  */ ||
                character == 0xD /* == '\r' == 13  */ ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }

        public static string Limit(this string s, int max, string ending = "...")
        {
            return s.Length > max ? "{0}{1}".FormatWith(s.Substring(0, max), ending) : s;
        }

        public static string PutSpacesInPascalCase(this string s, bool firstLetterToUpper = false)
        {
            var result = SpacesInPascalCaseRegex.Replace(s, m => $"{m.Groups["notcaps"].Value} {m.Groups["caps"].Value}");
            return firstLetterToUpper ? result.FirstLetterToUpper() : result;
        }

        private static readonly Regex SpacesInPascalCaseRegex = new Regex("(?'notcaps'[^A-Z^ ])(?'caps'[A-Z])", RegexOptions.Compiled);

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static string SafeReplace(this string value, string oldValue, string newValue)
        {
            if (value == null)
                return null;

            return value.Replace(oldValue, newValue);
        }

        public static string SafeGetFileName(this string fileName)
        {
            int length;
            if ((length = fileName.LastIndexOf('.')) == -1)
                return fileName;
            return StripQuerystring(fileName.Substring(0, length));
        }

        public static string SafeGetFileExtension(this string fileName)
        {
            int length;
            if ((length = fileName.LastIndexOf('.')) == -1)
                return fileName;
            return StripQuerystring(fileName.Substring(length, fileName.Length - length));
        }

        public static string StripQuerystring(this string input)
        {
            return input.Substring(0, input.IndexOf('?') != -1 ? input.IndexOf('?') : input.Length);
        }

        public static List<string> Wrap(this string s, int maxLength)
        {
            if (s.Length == 0) return new List<string>();

            var words = s.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var currentWord in words)
            {

                if ((currentLine.Length > maxLength) ||
                    ((currentLine.Length + currentWord.Length) > maxLength))
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }

                if (currentLine.Length > 0)
                    currentLine += " " + currentWord;
                else
                    currentLine += currentWord;

            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);


            return lines;
        }

        public static string EscapeForCsv(this string str)
        {
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        public static string ToTitleCase(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public static bool IsNumeric(this string s)
        {
            double d;
            return double.TryParse(s, out d);
        }

        public static string StripHtml(this string input)
        {
            input = Regex.Replace(input, "<.*?>", "\n");
            return Regex.Replace(input, "<.*?>", string.Empty).Trim();
        }
    }
}
