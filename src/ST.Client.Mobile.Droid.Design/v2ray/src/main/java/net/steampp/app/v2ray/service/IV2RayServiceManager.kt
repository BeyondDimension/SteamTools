package net.steampp.app.v2ray.service

interface IV2RayServiceManager {
    fun startSpeedNotification()

    fun cancelNotification()

    fun showNotification()

    fun getConfigureFileContent(): String

    fun getDomainName(): String

    fun getEnableLocalDns(): Boolean

    fun getForwardIpv6(): Boolean
}