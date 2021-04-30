using AndroidX.Fragment.App;
using Google.Android.Material.Tabs;

namespace System.Application.UI.Adapters
{
    public class ViewPagerWithTabLayoutAdapter : ViewPagerAdapter, TabLayoutMediator.ITabConfigurationStrategy
    {
        public new interface IHost : ViewPagerAdapter.IHost
        {
            string GetPageTitle(int position);
        }

        readonly IHost host;

        public ViewPagerWithTabLayoutAdapter(IHost host, Fragment fragment) : base(host, fragment)
        {
            this.host = host;
        }

        public ViewPagerWithTabLayoutAdapter(IHost host, FragmentActivity activity) : base(host, activity)
        {
            this.host = host;
        }

        void TabLayoutMediator.ITabConfigurationStrategy.OnConfigureTab(TabLayout.Tab tab, int position)
        {
            var title = host.GetPageTitle(position);
            tab.SetText(title);
        }
    }
}