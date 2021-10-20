using Android.Content;
using Android.Runtime;
using Android.Util;
using System;

namespace AndroidX.RecyclerView.Widget
{
    public class LinearLayoutManager2 : LinearLayoutManager
    {
        public LinearLayoutManager2(Context context) : base(context)
        {
        }

        public LinearLayoutManager2(Context context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
        {
        }

        public LinearLayoutManager2(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected LinearLayoutManager2(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                base.OnLayoutChildren(recycler, state);
            }
            catch (Java.Lang.IndexOutOfBoundsException)
            {
            }
        }
    }
}