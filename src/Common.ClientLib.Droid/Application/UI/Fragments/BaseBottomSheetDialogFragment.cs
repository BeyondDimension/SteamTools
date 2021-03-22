using Android.OS;
using Android.Views;
using Google.Android.Material.BottomSheet;

namespace System.Application.UI.Fragments
{
    /// <summary>
    /// 当前应用的 <see cref="BottomSheetDialogFragment"/>(底部弹窗) 基类
    /// </summary>
    public abstract partial class BaseBottomSheetDialogFragment : BottomSheetDialogFragment
    {
        /// <inheritdoc cref="IFragment.LayoutResource"/>
        protected abstract int? LayoutResource { get; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = IFragment.OnCreateView(this, inflater, container);
            if (view == null)
                return base.OnCreateView(inflater, container, savedInstanceState);
            mView = null;
            return view;
        }

        public abstract void OnCreateView(View view);

        /// <inheritdoc cref="IFragment.OnClick(View)"/>
        protected virtual bool OnClick(View view) => false;

        void View.IOnClickListener.OnClick(View? view) => IFragment.OnClick(this, view);

        /// <inheritdoc cref="IFragment.IsLightStatusBar"/>
        protected virtual bool? IsLightStatusBar => null;

        public override void OnResume()
        {
            base.OnResume();
            IFragment.ChangeStatusBar(this);
        }
    }
}