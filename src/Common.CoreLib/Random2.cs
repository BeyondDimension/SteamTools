using System.Common;
//using System.Security.Cryptography;

namespace System;

/// <summary>
/// 线程安全的随机数生成
/// <para>https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way</para>
/// </summary>
public static class Random2
{
    //static readonly RNGCryptoServiceProvider _global = new();

    [ThreadStatic]
    static Random? _local;

    static Random GetRandom()
    {
        var inst = _local;
        if (inst == null)
        {
            //byte[] buffer = new byte[4];
            //_global.GetBytes(buffer);
            //_local = inst = new Random(BitConverter.ToInt32(buffer, 0));

            // GUID生成随机数性能比RNGCryptoServiceProvider更好
            _local = inst = new Random(Guid.NewGuid().GetHashCode());
        }
        return inst;
    }

    /// <inheritdoc cref="RANDOM.Next"/>
    public static int Next() => GetRandom().Next();

    /// <inheritdoc cref="RANDOM.Next(int)"/>
    public static int Next(int maxValue) => GetRandom().Next(maxValue);

    /// <inheritdoc cref="RANDOM.Next(int, int)"/>
    public static int Next(int minValue, int maxValue) => GetRandom().Next(minValue, maxValue);

    /// <inheritdoc cref="RANDOM.NextDouble"/>
    public static double NextDouble() => GetRandom().NextDouble();

    static char RandomCharAt(string s, int index)
    {
        if (index == s.Length) index = 0;
        else if (index > s.Length) index %= s.Length;
        return s[index];
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="length">要生成的字符串长度</param>
    /// <param name="randomChars">随机字符串字符集</param>
    /// <returns></returns>
    public static string GenerateRandomString(int length = 6, string randomChars = Constants.DigitsLetters)
    {
        var random = GetRandom();
        var result = new char[length];
        if (random.Next(256) % 2 == 0)
            for (var i = length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(i);
        else
            for (var i = 0; i < length; i++) // 0 1 2 3 4 5
                EachGenerate(i);
        return new string(result);
        void EachGenerate(int i)
        {
            var index = random.Next(0, randomChars.Length);
            var temp = RandomCharAt(randomChars, index);
            result[i] = temp;
        }
    }

    /// <summary>
    /// 生成随机数字
    /// </summary>
    /// <param name="length">要生成的字符串长度</param>
    /// <param name="endIsZero">生成的数字最后一位是否能够为0，默认不能为0(<see langword="false"/>)</param>
    /// <returns></returns>
    public static int GenerateRandomNum(int length = 6, bool endIsZero = false)
    {
        var random = GetRandom();
        var result = 0;
        var lastNum = 0;
        if (random.Next(256) % 2 == 0)
            for (int i = length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(i);
        else
            for (int i = 0; i < length; i++) // 0 1 2 3 4 5
                EachGenerate(i);
        return result;
        void EachGenerate(int i)
        {
            var bit = (int)(i == 0 ? 1 : Math.Pow(10, i));
            // 100,000  10,000  1,000   100     10      1
            // 1        10      100     1,000   10,000  100,000
            var current = random.Next(lastNum + 1, lastNum + 10);
            lastNum = current % 10;
            if (lastNum == 0)
            {
                // i != 0 &&  i!=5 末尾和开头不能有零
                if ((i != 0 || endIsZero) && i != length - 1)
                    return;
                lastNum = random.Next(1, 10);
            }
            result += lastNum * bit;
        }
    }

    /// <summary>
    /// 从多个元素中随机获取一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static T GetRandomItem<T>(this IReadOnlyList<T> items) => items[Next(items.Count)];
}