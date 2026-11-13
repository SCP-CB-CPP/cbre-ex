using System;
using System.Collections.Generic;

namespace CBRE.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Split a string, but don't split within quoted values.
        /// </summary>
        /// <param name="line">The line to split</param>
        /// <returns>An array of split values</returns>
        public static IReadOnlyList<string> SplitWithQuotes(this string line)
        {
            var result = new List<string>();
            var inQuoteStart = -1;
            for (var i = 0; i < line.Length; i++) {
                var isQuote = line[i] == '"';
                if (isQuote) {
                    if (inQuoteStart >= 0) {
                        result.Add(line.Substring(inQuoteStart, i - inQuoteStart));
                        inQuoteStart = -1;
                    } else {
                        inQuoteStart = i + 1;
                    }
                }
            }
            if (result.Count == 0) {
                return [line];
            }
            return result;
        }
        
        public static bool ToBool(this string value)
        {
            string lowercaseValue = value.ToLower();

            return lowercaseValue == "1" || lowercaseValue == "yes" || lowercaseValue == "true";
        }
    }
}
