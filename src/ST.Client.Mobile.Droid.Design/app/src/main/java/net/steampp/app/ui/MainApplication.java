package net.steampp.app.ui;

import android.app.Application;
import android.content.Context;

public class MainApplication extends Application {
    static Context _context;

    public static Context getContext() {
        return _context;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        _context = this;
    }
}
