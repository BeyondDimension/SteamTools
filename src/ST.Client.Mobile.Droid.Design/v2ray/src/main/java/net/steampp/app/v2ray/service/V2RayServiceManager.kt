// This source file is adapted from the v2rayNG project.
// https://github.com/2dust/v2rayNG/blob/master/V2rayNG/app/src/main/kotlin/com/v2ray/ang/service/V2RayServiceManager.kt
// Licensed to The GPL-3.0 License.

package net.steampp.app.v2ray.service

import android.util.Log
import go.Seq
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import libv2ray.Libv2ray
import libv2ray.V2RayPoint
import libv2ray.V2RayVPNServiceSupportsSet
import net.steampp.app.v2ray.util.Utils
import java.lang.ref.SoftReference
import net.steampp.app.v2ray.AppConfig.ANG_PACKAGE

object V2RayServiceManager {
    private val v2rayPoint: V2RayPoint = Libv2ray.newV2RayPoint(V2RayCallback())

    fun getV2RayPoint(): V2RayPoint {
        return v2rayPoint
    }

    internal var serviceControl: SoftReference<ServiceControl>? = null
        set(value) {
            field = value
            val context = value?.get()?.getService()?.applicationContext
            context?.let {
                v2rayPoint.packageName = Utils.packagePath(context)
                v2rayPoint.packageCodePath = context.applicationInfo.nativeLibraryDir + "/"
                Seq.setContext(context)
            }
        }

    private var lastQueryTime = 0L

    private class V2RayCallback : V2RayVPNServiceSupportsSet {
        override fun shutdown(): Long {
            val serviceControl = serviceControl?.get() ?: return -1
            // called by go
            // shutdown the whole vpn service
            return try {
                serviceControl.stopService()
                0
            } catch (e: Exception) {
                Log.d(ANG_PACKAGE, e.toString())
                -1
            }
        }

        override fun prepare(): Long {
            return 0
        }

        override fun protect(l: Long): Long {
            val serviceControl = serviceControl?.get() ?: return 0
            return if (serviceControl.vpnProtect(l.toInt())) 0 else 1
        }

        override fun onEmitStatus(l: Long, s: String?): Long {
            //Logger.d(s)
            return 0
        }

        override fun setup(s: String): Long {
            val serviceControl = serviceControl?.get() ?: return -1
            //Logger.d(s)
            return try {
                serviceControl.startService(s)
                lastQueryTime = System.currentTimeMillis()
                startSpeedNotification()
                0
            } catch (e: Exception) {
                Log.d(ANG_PACKAGE, e.toString())
                -1
            }
        }

    }

    internal fun startV2rayPoint() {
        var i = getInterface() ?: return
//        val service = serviceControl?.get()?.getService() ?: return
//        val guid = mainStorage?.decodeString(MmkvManager.KEY_SELECTED_SERVER) ?: return
//        val config = MmkvManager.decodeServerConfig(guid) ?: return
        if (!v2rayPoint.isRunning) {
//            val result = V2rayConfigUtil.getV2rayConfig(service, guid)
//            if (!result.status)
//                return
//
//            try {
//                val mFilter = IntentFilter(AppConfig.BROADCAST_ACTION_SERVICE)
//                mFilter.addAction(Intent.ACTION_SCREEN_ON)
//                mFilter.addAction(Intent.ACTION_SCREEN_OFF)
//                mFilter.addAction(Intent.ACTION_USER_PRESENT)
//                service.registerReceiver(mMsgReceive, mFilter)
//            } catch (e: Exception) {
//                Log.d(ANG_PACKAGE, e.toString())
//            }

            v2rayPoint.configureFileContent = i.getConfigureFileContent()
            v2rayPoint.domainName = i.getDomainName()
//            currentConfig = config
            v2rayPoint.enableLocalDNS = i.getEnableLocalDns()
            v2rayPoint.forwardIpv6 = i.getForwardIpv6()
            v2rayPoint.proxyOnly = false

            try {
                v2rayPoint.runLoop()
            } catch (e: Exception) {
                Log.d(ANG_PACKAGE, e.toString())
            }

            if (v2rayPoint.isRunning) {
//                MessageUtil.sendMsg2UI(service, AppConfig.MSG_STATE_START_SUCCESS, "")
                i.showNotification()
            } else {
//                MessageUtil.sendMsg2UI(service, AppConfig.MSG_STATE_START_FAILURE, "")
                i.cancelNotification()
            }
        }
    }

    internal fun stopV2rayPoint() {
        var i = getInterface() ?: return

        if (v2rayPoint.isRunning) {
            GlobalScope.launch(Dispatchers.Default) {
                try {
                    v2rayPoint.stopLoop()
                } catch (e: Exception) {
                    Log.d(ANG_PACKAGE, e.toString())
                }
            }
        }

//        MessageUtil.sendMsg2UI(service, AppConfig.MSG_STATE_STOP_SUCCESS, "")
        i.cancelNotification()

//        try {
//            service.unregisterReceiver(mMsgReceive)
//        } catch (e: Exception) {
//            Log.d(ANG_PACKAGE, e.toString())
//        }
    }

    private fun getInterface(): IV2RayServiceManager? {
        val s = serviceControl?.get();
        if (s is IV2RayServiceManager) {
            return s;
        }
        return null
    }

    private fun startSpeedNotification() {
        getInterface()?.startSpeedNotification()
    }
}