using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using System.Application.UI.Adapters;

namespace System.Application.UI.Fragments
{
    /// <summary>
    /// ViewPager2 + TabLayout的 <see cref="Fragment"/> 基类
    /// </summary>
    public abstract class ViewPagerWithTabLayoutFragment : BaseFragment, ViewPagerWithTabLayoutAdapter.IHost
    {
        public ViewPagerWithTabLayoutAdapter? Adapter { get; private set; }

        protected abstract ViewPager2 ViewPager { get; }

        protected abstract TabLayout TabLayout { get; }

        public override void OnCreateView(View view)
        {
            Adapter = new ViewPagerWithTabLayoutAdapter(this, this);
            ViewPager.SetupWithTabLayout(TabLayout, Adapter);
        }

        protected abstract string GetPageTitle(int position);

        protected abstract Fragment CreateFragment(int position);

        protected virtual int ItemCount => 2;

        string ViewPagerWithTabLayoutAdapter.IHost.GetPageTitle(int position) => GetPageTitle(position);

        Fragment ViewPagerAdapter.IHost.CreateFragment(int position) => CreateFragment(position);

        int ViewPagerAdapter.IHost.ItemCount => ItemCount;
    }
}