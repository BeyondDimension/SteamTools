using Android.Views;

namespace System.Application.UI.Adapters
{
    internal class PlatformItemClickEventArgs<TViewModel> : EventArgs, ItemClickEventArgs<TViewModel>
    {
        readonly View.LongClickEventArgs? longClickEventArgs;

        public PlatformItemClickEventArgs(View view, int position, TViewModel current, View.LongClickEventArgs? longClickEventArgs)
        {
            View = view;
            Position = position;
            Current = current;
            this.longClickEventArgs = longClickEventArgs;
        }

        public View View { get; }

        public int Position { get; }

        public TViewModel Current { get; }

        public bool Handled
        {
            get
            {
                return longClickEventArgs?.Handled ?? default;
            }
            set
            {
                if (longClickEventArgs != null) longClickEventArgs.Handled = value;
            }
        }
    }
}