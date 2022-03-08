package net.steampp.app.v2ray.service

interface IV2RayServiceManager {
//    fun cancelNotification()

//    fun showNotification()

    fun getConfigureFileContent(): String

    fun getDomainName(): String

    fun getEnableLocalDns(): Boolean

    fun getForwardIpv6(): Boolean

    fun updateNotification(contentText: String?, proxyTraffic: Long, directTraffic: Long)
}