package net.steampp.app.ui

import android.app.Application
import android.content.Context

class MainApplication : Application() {
    override fun onCreate() {
        super.onCreate()
        context = this
    }

    companion object {
        var context: Context? = null
    }
}
