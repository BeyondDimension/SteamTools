//using System.Properties;

//namespace System.Application
//{
//    /// <summary>
//    /// 性别
//    /// </summary>
//    public enum Gender : byte
//    {
//        /// <summary>
//        /// 未知
//        /// </summary>
//        Unknown = 0,

//        /// <summary>
//        /// 男性
//        /// </summary>
//        Male = 1,

//        /// <summary>
//        /// 女性
//        /// </summary>
//        Female = 2,
//    }

//    /// <summary>
//    /// Enum 扩展 <see cref="Gender"/>
//    /// </summary>
//    public static partial class GenderEnumExtensions
//    {
//        /// <summary>
//        /// 性别为 男(Male) 或 女(Female)
//        /// </summary>
//        /// <param name="gender"></param>
//        /// <returns></returns>
//        public static bool IsMaleOrFemale(this Gender gender)
//            => gender == Gender.Male || gender == Gender.Female;

//        /// <summary>
//        /// 转换为 显示用字符串 男(Male) / 女(Female) / 未知(Unknown)
//        /// </summary>
//        /// <param name="gender"></param>
//        /// <param name="default"></param>
//        /// <returns></returns>
//        /// <exception cref="ArgumentOutOfRangeException"></exception>
//        public static string ToStringDisplay(this Gender gender, string? @default = null)
//        {
//            return gender switch
//            {
//                Gender.Male => SR.Male,
//                Gender.Female => SR.Female,
//                Gender.Unknown => @default ?? SR.Unknown,
//                _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null),
//            };
//        }

//        /// <summary>
//        /// 转换为 第三人称 字符串 他(He) / 她(She) / 未知(Unknown)
//        /// </summary>
//        /// <param name="gender"></param>
//        /// <param name="default"></param>
//        /// <returns></returns>
//        public static string ToStringThirdPerson(this Gender gender, Gender? @default = null)
//        {
//            return gender switch
//            {
//                Gender.Male => SR.He,
//                Gender.Female => SR.She,
//                Gender.Unknown => (!@default.HasValue || @default.Value == Gender.Unknown) ? SR.Unknown : ToStringThirdPerson(@default.Value),
//                _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null),
//            };
//        }
//    }
//}