/*
 * Copyright (C) 2011 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using static System.Application.Models.GAPAuthenticatorValueDTO;

namespace System.Application.Models
{
    partial interface IGAPAuthenticatorValueDTO
    {
        /// <summary>
        /// 使用当前加密和/或密码保存的当前数据（可能为无）
        /// </summary>
        string? EncryptedData { get; set; }

        /// <summary>
        /// 服务器时间戳
        /// </summary>
        long ServerTime { get; }

        /// <summary>
        /// 用于加密机密数据的密码类型
        /// </summary>
        PasswordTypes PasswordType { get; set; }

        bool RequiresPassword { get; }

        /// <summary>
        /// 发行者名称
        /// </summary>
        string? Issuer { get; set; }

        /// <summary>
        /// 获取/设置组合的机密数据值
        /// </summary>
        string? SecretData { get; set; }

        /// <summary>
        /// 根据计算的服务器时间计算代码间隔
        /// </summary>
        long CodeInterval { get; }

        /// <summary>
        /// 获取验证器的当前代码
        /// </summary>
        string CurrentCode { get; }

        /// <summary>
        /// 将此验证器的时间与服务器时间同步。我们用UTC时间的差值更新数据记录
        /// </summary>
        void Sync();

        void Protect();

        bool Unprotect(string? password);
    }
}