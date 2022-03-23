// This source file is adapted from the v2rayNG project.
// https://github.com/2dust/v2rayNG/blob/master/V2rayNG/app/src/main/kotlin/com/v2ray/ang/service/ServiceControl.kt
// Licensed to The GPL-3.0 License.

package net.steampp.app.v2ray.service

import android.app.Service

internal interface ServiceControl {
    fun getService(): Service

    fun startService(parameters: String)

    fun stopService()

    fun vpnProtect(socket: Int): Boolean

}