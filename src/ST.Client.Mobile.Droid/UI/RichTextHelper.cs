using Android.Text;
using System.Collections.Generic;
using System.Text;
using JObject = Java.Lang.Object;

namespace System.Application.UI
{
    public static class RichTextHelper
    {
        public static SpannableString CreateSpannableString(Func<List<(JObject what, int start, int end, SpanTypes flags)>, StringBuilder> func)
        {
            var linkTextIndexs = new List<(JObject what, int start, int end, SpanTypes flags)>();
            var sb = func(linkTextIndexs);
            var str = sb.ToString();
            SpannableString spannable = new(str);
            foreach (var (what, start, end, flags) in linkTextIndexs)
            {
                spannable.SetSpan(what, start, end, flags);
            }
            return spannable;
        }
    }
}