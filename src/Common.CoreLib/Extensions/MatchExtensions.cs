using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class MatchExtensions
    {
        public static string GetValue(this Match match, Func<Match, bool> action)
        {
            if (action.Invoke(match)) return match.Value.Trim();
            else return "";
        }

        public static IEnumerable<string> GetValues(this MatchCollection match, Func<Match, bool> action)
        {
            foreach (Match item in match)
            {
                if (action.Invoke(item))
                    yield return item.Value.Trim();
            }
        }
    }
}