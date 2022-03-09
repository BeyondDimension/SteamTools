// This source file is adapted from the shadowsocks-android project.
// https://github.com/shadowsocks/shadowsocks-android/blob/master/core/src/main/java/com/github/shadowsocks/Core.kt
// Licensed to The GPL-3.0 License.
/*******************************************************************************
 *                                                                             *
 *  Copyright (C) 2018 by Max Lv <max.c.lv@gmail.com>                          *
 *  Copyright (C) 2018 by Mygod Studio <contact-shadowsocks-android@mygod.be>  *
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

package net.steampp.app.shadowsocks

import android.app.Application
import android.os.Build
import androidx.annotation.VisibleForTesting
import net.steampp.app.shadowsocks.utils.DeviceStorageApp

object Core {
    internal const val TAG = "shadowsocks"
    lateinit var app: Application
        @VisibleForTesting set
    val deviceStorage by lazy { if (Build.VERSION.SDK_INT < 24) app else DeviceStorageApp(app) }
//    val user by lazy { app.getSystemService<UserManager>()!! }

    fun init(app: Application) {
        this.app = app
    }
}