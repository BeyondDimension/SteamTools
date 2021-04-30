using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;

namespace System.Application.UI.Adapters
{
    public class ViewPagerAdapter : FragmentStateAdapter
    {
        public interface IHost
        {
            Fragment CreateFragment(int position);

            int ItemCount { get; }
        }

        readonly IHost host;

        public ViewPagerAdapter(IHost host, Fragment fragment) : base(fragment)
        {
            this.host = host;
        }

        public ViewPagerAdapter(IHost host, FragmentActivity activity) : base(activity)
        {
            this.host = host;
        }

        public override Fragment CreateFragment(int position) => host.CreateFragment(position);

        public override int ItemCount => host.ItemCount;
    }
}