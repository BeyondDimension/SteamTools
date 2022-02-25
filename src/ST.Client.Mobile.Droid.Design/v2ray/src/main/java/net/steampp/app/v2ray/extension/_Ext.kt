// This source file is adapted from the v2rayNG project.
// https://github.com/2dust/v2rayNG/blob/master/V2rayNG/app/src/main/kotlin/com/v2ray/ang/extension/_Ext.kt
// Licensed to The GPL-3.0 License.

package net.steampp.app.v2ray.extension

/**
 * Some extensions
 */

const val threshold = 1000
const val divisor = 1024F

fun Long.toSpeedString() = toTrafficString() + "/s"

fun Long.toTrafficString(): String {
    if (this == 0L)
        return "\t\t\t0\t  B"

    if (this < threshold)
        return "${this.toFloat().toShortString()}\t  B"

    val kib = this / divisor
    if (kib < threshold)
        return "${kib.toShortString()}\t KB"

    val mib = kib / divisor
    if (mib < threshold)
        return "${mib.toShortString()}\t MB"

    val gib = mib / divisor
    if (gib < threshold)
        return "${gib.toShortString()}\t GB"

    val tib = gib / divisor
    if (tib < threshold)
        return "${tib.toShortString()}\t TB"

    val pib = tib / divisor
    if (pib < threshold)
        return "${pib.toShortString()}\t PB"

    val eib = pib / divisor
    if (eib < threshold)
        return "${eib.toShortString()}\t EB"

    val zib = eib / divisor
    if (zib < threshold)
        return "${zib.toShortString()}\t ZB"

    return "∞"
}

private fun Float.toShortString(): String {
    val s = "%.2f".format(this)
    if (s.length <= 4)
        return s
    return s.substring(0, 4).removeSuffix(".")
}