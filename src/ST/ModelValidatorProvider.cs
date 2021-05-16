/* (模型验证提供者)模块说明
 * 模型类 应当继承接口 IReadOnlyXXX 或 IReadOnlyNullableXXX
 * IXXX接口 不继承 IReadOnly 或 IReadOnlyNullable
 * 所以 应当 使用 IReadOnlyXXX 或 IReadOnlyNullableXXX
 * IXXX 实现 get, set 常用于表中的字段
 * IReadOnlyXXX 仅实现get，用于验证字段值是否正确
 * IReadOnlyNullableXXX 基本同上，不同在于允许 null 值，如果字段类型有Empty，则通常Empty等效于null
 * ----------------------------------------------------------------------
 * 添加一个新的列的验证，例：
 * 定义一个 IXXX接口
 * 定义一个 IReadOnlyXXX接口
 * 如果允许null值，则再定义一个 IReadOnlyNullableXXX 接口
 * ----------------------------------------------------------------------
 * 验证过滤字段参数 params Type[] ignores
 * 传入的类型 为接口 IXXX 或 IReadOnlyXXX 或 IReadOnlyNullableXXX 三者等效
 * 实现逻辑：通过反射Name字符串截取获取XXX作为比较
 * ----------------------------------------------------------------------
 * 对于模型类中某一个字段XXX，要么此类继承 IReadOnlyXXX 要么继承 IReadOnlyNullableXXX 不能同时继承两者
 * ----------------------------------------------------------------------
 * 如果对于某一个模型类中的某个类型，需要对另一个字段进行不同的验证，可添加新的类型字段接口
 * 在 ColumnValidate 验证逻辑中 添加新的 is IXXX t switch(t.X) 进行不同的验证
 * ----------------------------------------------------------------------
 * 如果需要根据不同的模型类，进行输出不同的 errorMessage 原理同上
 */

using System.Application.Columns;
using System.Application.Models;
using System.Application.Properties;
using static System.Application.Services.Implementation.ModelValidator;

namespace System.Application
{
    /// <summary>
    /// 模型验证提供者
    /// </summary>
    public static class ModelValidatorProvider
    {
        public static void Init()
        {
            AddColumnValidate<IExplicitHasValue>(x =>
            {
                if (!x.HasValue()) return Constants.内容值不能为空或不正确;
                return null;
            });
            AddColumnValidate<IReadOnlyPhoneNumber>(ColumnValidate);
            AddColumnValidate<IReadOnlySmsCode>(ColumnValidate);
            //AddColumnValidate<IReadOnlyBirthDate>(ColumnValidate);
            //AddColumnValidate<IReadOnlyGender>(ColumnValidate);
            AddColumnValidate<IReadOnlyAvatar>(ColumnValidate);
            AddColumnValidate<IReadOnlyNickName>(ColumnValidate);
        }

        #region ColumnValidates

        static string? ColumnValidate(IReadOnlyPhoneNumber value)
        {
            if (string.IsNullOrEmpty(value.PhoneNumber))
            {
                return Constants.请输入手机号码哦;
            }
            if (!IsPhoneNumberCorrect(value.PhoneNumber))
            {
                return Constants.请输入正确的手机号码哦;
            }
            return null;
        }

        static string? ColumnValidate(IReadOnlySmsCode value)
        {
            if (string.IsNullOrEmpty(value.SmsCode))
            {
                return Constants.请输入短信验证码哦;
            }
            if (!IsSmsCodeCorrect(value.SmsCode))
            {
                return Constants.短信验证码不正确;
            }
            return null;
        }

        //static string? ColumnValidate(IReadOnlyBirthDate value)
        //{
        //    if (value.BirthDate == default)
        //    {
        //        return Constants.请输入年龄;
        //    }
        //    return null;
        //}

        //static string? ColumnValidate(IReadOnlyGender value)
        //{
        //    if (!value.Gender.IsDefined())
        //    {
        //        return Constants.请选择性别;
        //    }
        //    return null;
        //}

        static string? ColumnValidate(IReadOnlyAvatar value)
        {
            if (value.Avatar == default)
            {
                return Constants.请选择头像;
            }
            return null;
        }

        static string? ColumnValidate(IReadOnlyNickName value)
        {
            if (string.IsNullOrWhiteSpace(value.NickName))
            {
                return Constants.请输入昵称;
            }
            else if (value.NickName.Length > Lengths.NickName)
            {
                return Constants.昵称最大长度不能超过_.Format(Lengths.NickName);
            }
            return null;
        }

        #endregion

        #region Validates

        /// <summary>
        /// 验证字符串是否为正确的手机号码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static bool IsPhoneNumberCorrect(string value)
            // 纯数字， 1开头，11位手机号码
            => !(value.Length != Lengths.PhoneNumber || value[0] != '1' || !value.IsDigital());

        /// <summary>
        /// 验证字符串是否为正确的短信验证码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static bool IsSmsCodeCorrect(string value)
            => !(value.Length != Lengths.SMS_CAPTCHA || !value.IsDigital());

        #endregion

        public static class Constants
        {
            public static string 内容值不能为空或不正确 => SR.内容值不能为空或不正确;

            public static string 请输入手机号码哦 => SR.请输入手机号码哦;

            public static string 请输入正确的手机号码哦 => SR.请输入正确的手机号码哦;

            public static string 请输入短信验证码哦 => SR.请输入短信验证码哦;

            public static string 短信验证码不正确 => SR.短信验证码不正确;

            public static string 请选择头像 => SR.请选择头像;

            public static string 请输入昵称 => SR.请输入昵称;

            public static string 昵称最大长度不能超过_ => SR.昵称最大长度不能超过_;

            public static string 请输入邀请码哦 => SR.请输入邀请码哦;

            public static string 请输入正确的邀请码哦 => SR.请输入正确的邀请码哦;

            public static string 手机号码已存在_注册 => SR.手机号码已存在_注册;

            public static string 手机号码已存在_换绑手机 => SR.手机号码已存在_换绑手机;

            public static string 手机号码已存在 => SR.手机号码已存在;

            public static string 用户不存在 => SR.用户不存在;

            public static string 当前手机号发送短信过于频繁 => SR.当前手机号发送短信过于频繁;

            public static string 当前手机号今日发送短信数量超过最大上限 => SR.当前手机号今日发送短信数量超过最大上限;

            public static string 短信服务故障 => SR.短信服务故障;

            public static string 验证码已过期或不存在 => SR.验证码已过期或不存在;

            public static string 验证码不正确 => SR.验证码不正确;

            public static string 新手机号不能与旧手机号一样 => SR.新手机号不能与旧手机号一样;

            public static string 当前手机号码不存在 => SR.当前手机号码不存在;

            public static string AuthorizationFailErrorMessage_ => SR.AuthorizationFailErrorMessage_;

            public static string UserIsBanErrorMessage => SR.UserIsBanErrorMessage;

            public static string BindFail_UserIsNotNull => SR.BindFail_UserIsNotNull;
        }

        public static class Lengths
        {
            /// <summary>
            /// 颜色16进制值，#AARRGGBB
            /// </summary>
            public const int ColorHex = 9;

            /// <summary>
            /// 手机号码
            /// </summary>
            public const int PhoneNumber = PhoneNumberHelper.ChineseMainlandPhoneNumberLength;

            /// <summary>
            /// 昵称
            /// </summary>
            public const int NickName = 20;

            /// <summary>
            /// 短信验证码
            /// </summary>
            public const int SMS_CAPTCHA = 6;
        }
    }
}