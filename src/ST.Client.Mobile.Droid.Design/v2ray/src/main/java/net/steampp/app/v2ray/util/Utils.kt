// This source file is adapted from the v2rayNG project.
// https://github.com/2dust/v2rayNG/blob/master/V2rayNG/app/src/main/kotlin/com/v2ray/ang/util/Utils.kt
// Licensed to The GPL-3.0 License.

package net.steampp.app.v2ray.util

import android.content.Context

object Utils {
    /**
     * package path
     */
    fun packagePath(context: Context): String {
        var path = context.filesDir.toString()
        path = path.replace("files", "")
        //path += "tun2socks"

        return path
    }
}