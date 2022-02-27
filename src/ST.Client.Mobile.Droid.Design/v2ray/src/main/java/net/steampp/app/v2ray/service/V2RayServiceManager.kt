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
import net.steampp.app.v2ray.AppConfig.TAG_AGENT
import net.steampp.app.v2ray.AppConfig.TAG_BLOCKED
import net.steampp.app.v2ray.AppConfig.TAG_DIRECT
import net.steampp.app.v2ray.extension.toSpeedString
import rx.Observable
import rx.Subscription
import kotlin.math.min

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
    private var mSubscription: Subscription? = null

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
                Log.e(ANG_PACKAGE, e.toString())
            }

            if (v2rayPoint.isRunning) {
//                MessageUtil.sendMsg2UI(service, AppConfig.MSG_STATE_START_SUCCESS, "")
//                i.showNotification()
            } else {
//                MessageUtil.sendMsg2UI(service, AppConfig.MSG_STATE_START_FAILURE, "")
//                cancelNotification()
                val service = serviceControl?.get()?.getService()
                service?.stopSelf()
                // 启动 v2rayPoint 失败，停止当前服务
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
        cancelNotification()

//        try {
//            service.unregisterReceiver(mMsgReceive)
//        } catch (e: Exception) {
//            Log.d(ANG_PACKAGE, e.toString())
//        }
    }

    private fun cancelNotification() {
        mSubscription?.unsubscribe()
        mSubscription = null
        val service = serviceControl?.get()?.getService() ?: return
        service.stopForeground(true)
//        getInterface()?.cancelNotification()
    }

    private fun updateNotification(contentText: String?, proxyTraffic: Long, directTraffic: Long) {
        getInterface()?.updateNotification(contentText, proxyTraffic, directTraffic)
    }

    private fun startSpeedNotification() {
        if (mSubscription == null &&
            v2rayPoint.isRunning
        ) {
            var lastZeroSpeed = false
            val outboundTags = getAllOutboundTags()
            outboundTags?.remove(TAG_DIRECT)

            mSubscription = Observable.interval(3, java.util.concurrent.TimeUnit.SECONDS)
                .subscribe {
                    val queryTime = System.currentTimeMillis()
                    val sinceLastQueryInSeconds = (queryTime - lastQueryTime) / 1000.0
                    var proxyTotal = 0L
                    val text = StringBuilder()
                    outboundTags?.forEach {
                        val up = v2rayPoint.queryStats(it, "uplink")
                        val down = v2rayPoint.queryStats(it, "downlink")
                        if (up + down > 0) {
                            appendSpeedString(
                                text,
                                it,
                                up / sinceLastQueryInSeconds,
                                down / sinceLastQueryInSeconds
                            )
                            proxyTotal += up + down
                        }
                    }
                    val directUplink = v2rayPoint.queryStats(TAG_DIRECT, "uplink")
                    val directDownlink = v2rayPoint.queryStats(TAG_DIRECT, "downlink")
                    val zeroSpeed = (proxyTotal == 0L && directUplink == 0L && directDownlink == 0L)
                    if (!zeroSpeed || !lastZeroSpeed) {
                        if (proxyTotal == 0L) {
                            appendSpeedString(text, outboundTags?.firstOrNull(), 0.0, 0.0)
                        }
                        appendSpeedString(
                            text, TAG_DIRECT, directUplink / sinceLastQueryInSeconds,
                            directDownlink / sinceLastQueryInSeconds
                        )
                        updateNotification(
                            text.toString(),
                            proxyTotal,
                            directDownlink + directUplink
                        )
                    }
                    lastZeroSpeed = zeroSpeed
                    lastQueryTime = queryTime
                }
        }
    }

    private fun appendSpeedString(text: StringBuilder, name: String?, up: Double, down: Double) {
        var n = name ?: "no tag"
        n = n.substring(0, min(n.length, 6))
        text.append(n)
        for (i in n.length..6 step 2) {
            text.append("\t")
        }
        text.append("•  ${up.toLong().toSpeedString()}↑  ${down.toLong().toSpeedString()}↓\n")
    }

    private fun getInterface(): IV2RayServiceManager? {
        val s = serviceControl?.get();
        if (s is IV2RayServiceManager) {
            return s;
        }
        return null
    }

    private fun getAllOutboundTags(): MutableList<String> {
        return mutableListOf(TAG_AGENT, TAG_DIRECT, TAG_BLOCKED)
    }
}