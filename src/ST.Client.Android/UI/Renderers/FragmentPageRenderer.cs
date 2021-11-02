using Android.Content;
using Android.Views;
using AndroidX.Fragment.App;
using System.Application.UI.Fragments;
using System.Application.UI.Renderers;
using System.Application.UI.Views;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LocalAuthPage), typeof(FragmentPageRenderer<LocalAuthFragment>))]
[assembly: ExportRenderer(typeof(ArchiSteamFarmPlusPage), typeof(FragmentPageRenderer<ASFPlusFragment>))]
[assembly: ExportRenderer(typeof(MyPage), typeof(FragmentPageRenderer<ASFPlusFragment>))]

namespace System.Application.UI.Renderers
{
    internal class FragmentPageRenderer<TFragment> : PageRenderer where TFragment : Fragment, new()
    {
        public TFragment? Fragment { get; private set; } = new TFragment();

        public FormsAppCompatActivity Activity { get; }

        public int FragmentContainerViewId { get; private set; }

        public FragmentContainerView? FragmentContainerView { get; private set; }

        public FragmentPageRenderer(Context context) : base(context)
        {
            if (context is FormsAppCompatActivity activity) Activity = activity;
            else throw new InvalidCastException("context is not FormsAppCompatActivity");
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                RemoveAllViews();
            }

            if (e.NewElement != null)
            {
                FragmentContainerViewId = GenerateViewId();
                AddView(FragmentContainerView = new FragmentContainerView(Context)
                {
                    Id = FragmentContainerViewId,
                });

                var f = GetShellFragmentContainer(Activity.SupportFragmentManager.Fragments);
                if (f != null)
                {
                    f.ChildFragmentManager
                        .BeginTransaction()
                        .SetReorderingAllowed(true)
                        .Add(FragmentContainerViewId, Fragment)
                        .Commit();
                }
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            if (FragmentContainerView != null)
            {
                FragmentContainerView.Measure(MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MakeMeasureSpec(b - t, MeasureSpecMode.Exactly));
                FragmentContainerView.Layout(0, 0, r - l, b - t);
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (FragmentContainerView != null)
            {
                FragmentContainerView.Measure(widthMeasureSpec, heightMeasureSpec);
                SetMeasuredDimension(FragmentContainerView.MeasuredWidth, FragmentContainerView.MeasuredHeight);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (FragmentContainerView != null)
                {
                    FragmentContainerView.Dispose();
                    FragmentContainerView = null;
                }
                if (Fragment != null)
                {
                    Fragment.Dispose();
                    Fragment = null;
                }
            }
            base.Dispose(disposing);
        }

        static int MakeMeasureSpec(int size, MeasureSpecMode mode)
        {
            return (int)(size + mode);
        }

        static Fragment? GetShellFragmentContainer(IEnumerable<Fragment> fragments)
        {
            foreach (var fragment in fragments)
            {
                if (fragment is ShellItemRenderer renderer)
                {
                    foreach (var fragment2 in renderer.ChildFragmentManager.Fragments)
                    {
                        if (fragment2 is ShellSectionRenderer renderer2)
                        {
                            foreach (var fragment3 in renderer2.ChildFragmentManager.Fragments)
                            {
                                if (fragment3.GetType().Name == "ShellFragmentContainer")
                                {
                                    return fragment3;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
