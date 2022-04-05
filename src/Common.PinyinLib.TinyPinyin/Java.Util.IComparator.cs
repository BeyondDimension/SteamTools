#if MONOANDROID || ANDROID
using Android.Runtime;

namespace Org.Ahocorasick.Interval
{
    partial class IntervalableComparatorByPosition
    {
        int Java.Util.IComparator.Compare(Java.Lang.Object? o1, Java.Lang.Object? o2)
        {
            return Compare(o1.JavaCast<IIntervalable>(), o2.JavaCast<IIntervalable>());
        }
    }

    partial class IntervalableComparatorBySize
    {
        int Java.Util.IComparator.Compare(Java.Lang.Object? o1, Java.Lang.Object? o2)
        {
            return Compare(o1.JavaCast<IIntervalable>(), o2.JavaCast<IIntervalable>());
        }
    }
}
#endif