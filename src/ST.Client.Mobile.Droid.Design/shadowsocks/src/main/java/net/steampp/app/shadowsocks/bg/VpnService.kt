// This source file is adapted from the shadowsocks-android project.
// https://github.com/shadowsocks/shadowsocks-android/blob/master/core/src/main/java/com/github/shadowsocks/bg/VpnService.kt
// Licensed to The GPL-3.0 License.
/*******************************************************************************
 *                                                                             *
 *  Copyright (C) 2017 by Max Lv <max.c.lv@gmail.com>                          *
 *  Copyright (C) 2017 by Mygod Studio <contact-shadowsocks-android@mygod.be>  *
 *                                                                             *
 *  This program is free software: you can redistribute it and/or modify       *
 *  it under the terms of the GNU General Public License as published by       *
 *  the Free Software Foundation, either version 3 of the License, or          *
 *  (at your option) any later version.                                        *
 *                                                                             *
 *  This program is distributed in the hope that it will be useful,            *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of             *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the              *
 *  GNU General Public License for more details.                               *
 *                                                                             *
 *  You should have received a copy of the GNU General Public License          *
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.       *
 *                                                                             *
 *******************************************************************************/

package net.steampp.app.shadowsocks.bg

import android.net.LocalSocket
import android.net.LocalSocketAddress
import android.os.ParcelFileDescriptor
import android.system.ErrnoException
import android.util.Log
import kotlinx.coroutines.*
import net.steampp.app.shadowsocks.Core
import net.steampp.app.shadowsocks.Core.TAG
import java.io.File
import java.io.FileDescriptor
import java.io.IOException
import android.net.VpnService as BaseVpnService

abstract class VpnService : BaseVpnService() {
    companion object {
        const val VPN_MTU = 1500
        const val PRIVATE_VLAN4_CLIENT = "172.19.0.1"
        const val PRIVATE_VLAN4_ROUTER = "172.19.0.2"
        const val PRIVATE_VLAN6_CLIENT = "fdfe:dcba:9876::1"
        const val PRIVATE_VLAN6_ROUTER = "fdfe:dcba:9876::2"
        const val KEY_PRIVATE_VLAN4_CLIENT = "PRIVATE_VLAN4_CLIENT";
        const val KEY_PRIVATE_VLAN4_ROUTER = "PRIVATE_VLAN4_ROUTER";
        const val KEY_PRIVATE_VLAN6_CLIENT = "PRIVATE_VLAN6_CLIENT";
        const val KEY_PRIVATE_VLAN6_ROUTER = "PRIVATE_VLAN6_ROUTER";
    }

    inner class NullConnectionException : NullPointerException() {
        override fun getLocalizedMessage() =
            "Failed to start VPN service. You might need to reboot your device."
    }

    private var processes: GuardedProcessPool? = null
    private var conn: ParcelFileDescriptor? = null

    open fun getMtu(): Int {
        return VPN_MTU
    }

    open fun getPrivateVLAN(key: String): String {
        return when (key) {
            KEY_PRIVATE_VLAN4_CLIENT -> PRIVATE_VLAN4_CLIENT
            KEY_PRIVATE_VLAN4_ROUTER -> PRIVATE_VLAN4_ROUTER
            KEY_PRIVATE_VLAN6_CLIENT -> PRIVATE_VLAN6_CLIENT
            KEY_PRIVATE_VLAN6_ROUTER -> PRIVATE_VLAN6_ROUTER
            else -> ""
        }
    }

    open fun getSession(): String {
        return ""
    }

    open fun onConfigure(builder: Builder) {

    }

    open fun isSupportIpv6(): Boolean {
        return true
    }

    abstract fun getSocksServerAddress(): String

    abstract fun getPortLocalDns(): String

    open suspend fun onStart() {
        startProcesses()
    }

    private suspend fun startProcesses() {
        sendFd(startVpn())
    }

    private suspend fun startVpn(): FileDescriptor {
        val mtu = getMtu();
        val PRIVATE_VLAN4_CLIENT = getPrivateVLAN(KEY_PRIVATE_VLAN4_CLIENT)
        val PRIVATE_VLAN4_ROUTER = getPrivateVLAN(KEY_PRIVATE_VLAN4_ROUTER)
        val PRIVATE_VLAN6_CLIENT = getPrivateVLAN(KEY_PRIVATE_VLAN6_CLIENT)
        val PRIVATE_VLAN6_ROUTER = getPrivateVLAN(KEY_PRIVATE_VLAN6_ROUTER)
        val builder = Builder()
            .setSession(getSession())
            .setMtu(mtu)
            .addAddress(PRIVATE_VLAN4_CLIENT, 30)
            .addDnsServer(PRIVATE_VLAN4_ROUTER)

        val isSupportIpv6 = isSupportIpv6();
        if (isSupportIpv6) builder.addAddress(PRIVATE_VLAN6_CLIENT, 126)

        onConfigure(builder)

        val conn = builder.establish() ?: throw NullConnectionException()
        this.conn = conn

        val cmd = arrayListOf(
            File(applicationInfo.nativeLibraryDir, Executable.TUN2SOCKS).absolutePath,
            "--netif-ipaddr", PRIVATE_VLAN4_ROUTER,
            "--socks-server-addr", getSocksServerAddress(),
            "--tunmtu", mtu.toString(),
            "--sock-path", "sock_path",
            "--dnsgw", "127.0.0.1:${getPortLocalDns()}",
            "--loglevel", "warning"
        )
        if (isSupportIpv6) {
            cmd += "--netif-ip6addr"
            cmd += PRIVATE_VLAN6_ROUTER
        }
        cmd += "--enable-udprelay"

        if (processes == null) {
            processes = GuardedProcessPool {
                Log.w(TAG, it)
                onStop()
            }
        }

        processes!!.start(cmd, onRestartCallback = {
            try {
                sendFd(conn.fileDescriptor)
            } catch (e: ErrnoException) {
                stopRunner()
            }
        })

        return conn.fileDescriptor
    }

    private suspend fun sendFd(fd: FileDescriptor) {
        var tries = 0
        val path = File(Core.deviceStorage.noBackupFilesDir, "sock_path").absolutePath
        while (true) try {
            delay(50L shl tries)
            LocalSocket().use { localSocket ->
                localSocket.connect(
                    LocalSocketAddress(
                        path,
                        LocalSocketAddress.Namespace.FILESYSTEM
                    )
                )
                localSocket.setFileDescriptorsForSend(arrayOf(fd))
                localSocket.outputStream.write(42)
            }
            return
        } catch (e: IOException) {
            if (tries > 5) throw e
            tries += 1
        }
    }

    open fun onStop() {
        stopRunner()
    }

    private fun stopRunner() {
        GlobalScope.launch(Dispatchers.Main.immediate) {
            // we use a coroutineScope here to allow clean-up in parallel
            coroutineScope {
                killProcesses(this)
            }
        }

    }

    private fun killProcesses(scope: CoroutineScope) {
        processes?.run {
            close(scope)
            processes = null
        }
    }
}
