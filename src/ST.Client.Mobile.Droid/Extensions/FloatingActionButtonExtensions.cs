using Com.Leinardi.Android.Speeddial;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class FloatingActionButtonExtensions
    {
        public static void ReplaceLabels<TEnum>(this IReadOnlyDictionary<TEnum, FabWithLabelView>? speedDialDict, Func<TEnum, string> getLabel)
        {
            if (!speedDialDict.Any_Nullable()) return;
            foreach (var x in speedDialDict!)
            {
                var label = getLabel(x.Key);
                x.Value.SpeedDialActionItem = x.Value.SpeedDialActionItemBuilder.SetLabel(label).Create();
            }
        }
    }
}