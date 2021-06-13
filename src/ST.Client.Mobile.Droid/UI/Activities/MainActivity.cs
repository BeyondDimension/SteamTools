using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Navigation;
using AndroidX.Navigation.UI;
using Binding;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(MainActivity))]
    [Activity(Theme = "@style/MainTheme",
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class MainActivity : BaseActivity<activity_main>
    {
        protected override int? LayoutResource => Resource.Layout.activity_main;

        protected override bool BackToHome => true;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var appBarConfiguration = new AppBarConfiguration.Builder(
                Resource.Id.navigation_local_auth,
                Resource.Id.navigation_community_fix,
                Resource.Id.navigation_game_list,
                Resource.Id.navigation_my)
                .Build();
            var navController = Navigation.FindNavController(this, Resource.Id.nav_host_fragment_activity_main);
            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);
            NavigationUI.SetupWithNavController(binding!.nav_view, navController);
        }
    }
}