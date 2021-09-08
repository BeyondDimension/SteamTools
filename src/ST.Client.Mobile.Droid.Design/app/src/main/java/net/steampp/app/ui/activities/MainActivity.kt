package net.steampp.app.ui.activities

import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import androidx.navigation.ui.AppBarConfiguration
import net.steampp.app.design.R
import androidx.navigation.Navigation
import androidx.navigation.ui.NavigationUI
import net.steampp.app.ui.databinding.ActivityMainBinding

class MainActivity : AppCompatActivity() {
    private var binding: ActivityMainBinding? = null
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding!!.root)

        // 底部导航(https://material.io/components/bottom-navigation)
        // - 需要从应用程序中的任何地方访问的顶级目的地
        // - 三到五个目的地
        // - 仅限移动或平板电脑
        // 导航抽屉(https://material.io/components/navigation-drawer)
        // - 具有五个或更多顶级目的地的应用
        // - 具有两个或两个以上级别的导航等级的应用程序
        // - 不相关的目的地之间的快速导航

        // Passing each menu ID as a set of Ids because each
        // menu should be considered as top level destinations.
        val appBarConfiguration = AppBarConfiguration.Builder(
            R.id.navigation_local_auth,
            R.id.navigation_community_fix,
            R.id.navigation_game_list,
            R.id.navigation_my
        )
            .build()
        val navController = Navigation.findNavController(this, R.id.nav_host_fragment_activity_main)
        NavigationUI.setupActionBarWithNavController(this, navController, appBarConfiguration)
        NavigationUI.setupWithNavController(binding!!.navView, navController)
    }
}
