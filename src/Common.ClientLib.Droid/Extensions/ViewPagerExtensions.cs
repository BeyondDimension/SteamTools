using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using System.Application.UI.Adapters;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// 对 <see cref="ViewPager2"/> 类的扩展函数集
    /// </summary>
    public static class ViewPagerExtensions
    {
        public static void SetupWithTabLayout(
            this ViewPager2 viewPager,
            TabLayout tabLayout,
            ViewPagerWithTabLayoutAdapter adapter)
        {
            viewPager.Adapter = adapter;
            // https://developer.android.google.cn/training/animation/vp2-migration#tablayout
            new TabLayoutMediator(tabLayout, viewPager, adapter).Attach();
        }
    }
}