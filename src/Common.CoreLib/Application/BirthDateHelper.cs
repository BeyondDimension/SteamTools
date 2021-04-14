using System.Application.Columns;
using System.Properties;

namespace System.Application
{
    public static class BirthDateHelper
    {
        /// <summary>
        /// 一年的天数
        /// </summary>
        const double AYearDays = 365.2425;

        /// <summary>
        /// 将年龄值转换为用于UI显示的字符串
        /// </summary>
        /// <param name="age"></param>
        /// <returns></returns>
        public static string ToString(byte age)
        {
            return string.Format(SR.Age_, age);
        }

        static byte AgeResultCorrect(int age)
        {
            if (age < 0)
            {
                return 0;
            }
            else if (age > IBirthDate.HumanMaxAge)
            {
                return IBirthDate.HumanMaxAge;
            }
            else
            {
                return (byte)age;
            }
        }

        /// <summary>
        /// (仅适用于客户端计算)根据生日计算年龄
        /// </summary>
        /// <param name="birthDate"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        static byte CalcAge(DateTime? birthDate, byte def = 0)
        {
            if (birthDate.HasValue)
            {
                var today = DateTime.Today;
                var birth = new DateTime(birthDate.Value.Year,
                    birthDate.Value.Month,
                    birthDate.Value.Day,
                    0, 0, 0,
                    today.Kind);
                var age = (int)((today - birth).TotalDays / AYearDays);
                return AgeResultCorrect(age);
            }
            return def;
        }

        /// <summary>
        /// (仅适用于服务端计算)根据生日及用户所在时区计算年龄
        /// </summary>
        /// <param name="timeZoneTicks"></param>
        /// <param name="birthDate"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        static byte CalcAge(long timeZoneTicks, DateTime? birthDate, byte def = 0)
        {
            if (birthDate.HasValue)
            {
                var offset = new TimeSpan(timeZoneTicks);
                var today = DateTimeOffset.Now;
                var birth = new DateTimeOffset(birthDate.Value.Year,
                    birthDate.Value.Month,
                    birthDate.Value.Day,
                    0, 0, 0,
                    offset);
                var age = (int)((today - birth).TotalDays / AYearDays);
                return AgeResultCorrect(age);
            }
            return def;
        }

        /// <summary>
        /// (仅适用于服务端计算)根据生日及用户所在时区计算年龄
        /// </summary>
        /// <param name="birthDate"></param>
        /// <returns></returns>
        public static byte CalcAge(this IBirthDateTimeZoneTicks birthDate)
            => CalcAge(birthDate.TimeZoneTicks, birthDate.BirthDate);

        /// <summary>
        /// (仅适用于客户端计算)根据生日计算年龄并转换为用于UI显示的字符串
        /// </summary>
        /// <param name="birthDate"></param>
        /// <returns></returns>
        public static string CalcAgeToString(this IBirthDateCalcAge birthDate)
        {
            var age = CalcAge(birthDate.BirthDate, birthDate.Age);
            return ToString(age);
        }
    }
}