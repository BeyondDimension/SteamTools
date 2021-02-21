using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using static System.Security.Cryptography.Hashs;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class UnitTestHelper
    {
        public static DictionaryCanRepeatKey<TKey, TElement> ToDictionaryCanRepeatKey<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => new DictionaryCanRepeatKey<TKey, TElement>(source.Select(x => (keySelector(x), elementSelector(x))).ToArray());

        public static class Strings
        {
            [Flags]
            public enum Type
            {
                All = SimplifiedChinese | English | Emoji,
                AllExcludeEmoji = SimplifiedChinese | English,
                SimplifiedChinese = 2,
                English = 4,
                Emoji = 8,
            }

            static readonly string[] values_SimplifiedChinese = new[] { CSB出师表, LTXJ兰亭集序 };
            static readonly string[] values_English = new[] { Unicorn, Fallout, AssassinSCreed, GameofThrones, DivinityOriginalSin2, Halo4, MassEffect2, MetalGearSolid, ResidentEvil, HalfLife2, CallofDutyModernWarfare, RedDeadRedemption, FarCry2, DragonAgeOrigins, Portal2, BatmanArkhamAsylum, GrandTheftAutoIV, TheElderScrollsVSkyrim, KaerMorhen };
            static readonly string[] values_Emoji = new[] { Emoji };

            public static List<string> GetValues(Type type = Type.All)
            {
                var list = new List<string>();
                if (type.HasFlag(Type.SimplifiedChinese)) list.AddRange(values_SimplifiedChinese);
                if (type.HasFlag(Type.English)) list.AddRange(values_English);
                if (type.HasFlag(Type.Emoji)) list.AddRange(values_Emoji);
                return list;
            }

            public static List<string> Values => GetValues();

            public static List<string> GetValues(int count, Type type = Type.All)
            {
                var values = GetValues(type);
                if (values.Count == count) return values;
                else if (values.Count > count) return values.OrderBy(x => Guid.NewGuid()).Take(count).ToList();
                else
                {
                    var add_count = count - values.Count;
                    var list = values.ToList();
                    for (int i = 0; i < add_count; i++)
                    {
                        list.Add(values[Random2.Next(values.Count)]);
                    }
                    return list;
                }
            }

            public const string CSB出师表 = "先帝创业未半而中道崩殂，今天下三分，益州疲弊，此诚危急存亡之秋也。然侍卫之臣不懈于内，忠志之士忘身于外者，盖追先帝之殊遇，欲报之于陛下也。诚宜开张圣听，以光先帝遗德，恢弘志士之气，不宜妄自菲薄，引喻失义，以塞忠谏之路也。\n" +
                    "\n" +
                    "宫中府中，俱为一体，陟罚臧否，不宜异同。若有作奸犯科及为忠善者，宜付有司论其刑赏，以昭陛下平明之理，不宜偏私，使内外异法也。\n" +
                    "\n" +
                    "侍中、侍郎郭攸之、费祎、董允等，此皆良实，志虑忠纯，是以先帝简拔以遗陛下。愚以为宫中之事，事无大小，悉以咨之，然后施行，必能裨补阙漏，有所广益。\n" +
                    "\n" +
                    "将军向宠，性行淑均，晓畅军事，试用于昔日，先帝称之曰能，是以众议举宠为督。愚以为营中之事，悉以咨之，必能使行阵和睦，优劣得所。\n" +
                    "\n" +
                    "亲贤臣，远小人，此先汉所以兴隆也；亲小人，远贤臣，此后汉所以倾颓也。先帝在时，每与臣论此事，未尝不叹息痛恨于桓、灵也。侍中、尚书、长史、参军，此悉贞良死节之臣，愿陛下亲之信之，则汉室之隆，可计日而待也。\n" +
                    "\n" +
                    "臣本布衣，躬耕于南阳，苟全性命于乱世，不求闻达于诸侯。先帝不以臣卑鄙，猥自枉屈，三顾臣于草庐之中，咨臣以当世之事，由是感激，遂许先帝以驱驰。后值倾覆，受任于败军之际，奉命于危难之间，尔来二十有一年矣。\n" +
                    "\n" +
                    "先帝知臣谨慎，故临崩寄臣以大事也。受命以来，夙夜忧叹，恐托付不效，以伤先帝之明，故五月渡泸，深入不毛。今南方已定，兵甲已足，当奖率三军，北定中原，庶竭驽钝，攘除奸凶，兴复汉室，还于旧都。此臣所以报先帝而忠陛下之职分也。至于斟酌损益，进尽忠言，则攸之、祎、允之任也。\n" +
                    "\n" +
                    "愿陛下托臣以讨贼兴复之效，不效，则治臣之罪，以告先帝之灵。若无兴德之言，则责攸之、祎、允等之慢，以彰其咎；陛下亦宜自谋，以咨诹善道，察纳雅言，深追先帝遗诏，臣不胜受恩感激。\n" +
                    "\n" +
                    "今当远离，临表涕零，不知所言。";

            public const string LTXJ兰亭集序 = "永和九年，岁在癸丑，暮春之初，会于会稽山阴之兰亭，修禊事也。群贤毕至，少长咸集。此地有崇山峻岭，茂林修竹，又有清流激湍，映带左右，引以为流觞曲水，列坐其次。虽无丝竹管弦之盛，一觞一咏，亦足以畅叙幽情。\n" +
                    "\n" +
                    "是日也，天朗气清，惠风和畅。仰观宇宙之大，俯察品类之盛，所以游目骋怀，足以极视听之娱，信可乐也。\n" +
                    "\n" +
                    "夫人之相与，俯仰一世。或取诸怀抱，悟言一室之内；或因寄所托，放浪形骸之外。虽趣舍万殊，静躁不同，当其欣于所遇，暂得于己，快然自足，不知老之将至；及其所之既倦，情随事迁，感慨系之矣。向之所欣，俯仰之间，已为陈迹，犹不能不以之兴怀，况修短随化，终期于尽！古人云：“死生亦大矣。”岂不痛哉！\n" +
                    "\n" +
                    "每览昔人兴感之由，若合一契，未尝不临文嗟悼，不能喻之于怀。固知一死生为虚诞，齐彭殇为妄作。后之视今，亦犹今之视昔，悲夫！故列叙时人，录其所述，虽世殊事异，所以兴怀，其致一也。后之览者，亦将有感于斯文。";

            public const string Emoji = "😀😉🤩🥵😈👻👾😸👏🤝👕💼";

            public const string Unicorn = "Unicorn originally appeared in Soulcalibur II as one of Raphael's weapons (going by the name of Reiterpallasch) until Soulcalibur III where it received a design change by having a unicorn-like hilt on the bottom. Unicorn was also the ultimate weapon for Pirates and Knights with the \"Rapier\" discipline (Pirates must level 10+ times to unlock the discipline, while it is 20+ for Knights). The weapon also returned in Soulcalibur IV and has a similar design to Raphael's 1P Flambert.";

            public const string Fallout = "War, war never changes.";

            public const string AssassinSCreed = "Nothing is true, everything is permitted.";

            public const string GameofThrones = "Valar Morghulis";

            public const string DivinityOriginalSin2 = "Glory is mine!";

            public const string Halo4 = "Wake Up, John";

            public const string MassEffect2 = "I’m Commander Shepard, and this is my favorite store on the Citadel!";

            public const string MetalGearSolid = "We’re not tools of the government or anyone else. Fighting was the only thing I was good at, but at least I always fought for what I believed in.";

            public const string ResidentEvil = "You were almost a Jill sandwich!";

            public const string HalfLife2 = "The right man in the wrong place can make all the difference in the world.";

            public const string CallofDutyModernWarfare = "The healthy human mind doesn’t wake up in the morning thinking this is its last day on Earth. But I think that’s a luxury, not a curse. To know you’re close to the end is a kind of freedom. Good time to take… inventory.";

            public const string RedDeadRedemption = "Some trees flourish, others die. Some cattle grow strong, others are taken by wolves. Some men are born rich enough and dumb enough to enjoy their lives. Ain’t nothing fair.";

            public const string FarCry2 = "You can’t break a man the way you break a dog or a horse. The harder you beat a man, the taller he stands.";

            public const string DragonAgeOrigins = "The world fears the inevitable plummet into the abyss. Watch for that moment and when it comes, do not hesitate to leap. It is only when you fall that you learn whether you can fly.";

            public const string Portal2 = "Most test subjects do experience some cognitive deterioration after a few months in suspension. Now you’ve been under for… quite a lot longer, and it’s not out of the question that you might have a very minor case of serious brain damage. But don’t be alarmed, alright? Although, if you do feel alarmed, try to hold onto that feeling because that is the proper reaction to being told you have brain damage.";

            public const string BatmanArkhamAsylum = "Tell me Bats. What are you scared of? Failing to save this cesspool of a city? Not finding the Commissioner in time? Me, in a thong?";

            public const string GrandTheftAutoIV = "War is where the young and stupid are tricked by the old and bitter into killing each other.";

            public const string TheElderScrollsVSkyrim = "I used to be an adventurer like you until I took an arrow to the knee.";

            public const string KaerMorhen = "Kaer Morhen";
        }

        public static class Hashs_V2019
        {
            static Crc32 CreateCRC32()
            {
                return Crc32.Create();
            }

            public class String
            {
                partial class Length
                {
                    public const int CRC32 = 8;
                }

                /// <summary>
                /// 计算CRC32值
                /// </summary>
                /// <param name="text"></param>
                /// <param name="isLower"></param>
                /// <returns></returns>
                public static string CRC32(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateCRC32(), isLower);

                /// <summary>
                /// 计算CRC32值
                /// </summary>
                /// <param name="buffer"></param>
                /// <param name="isLower"></param>
                /// <returns></returns>
                public static string CRC32(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateCRC32(), isLower);

                /// <summary>
                /// 计算CRC32值
                /// </summary>
                /// <param name="inputStream"></param>
                /// <param name="isLower"></param>
                /// <returns></returns>
                public static string CRC32(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateCRC32(), isLower);
            }

            public class ByteArray
            {
                /// <summary>
                /// 计算CRC32值
                /// </summary>
                /// <param name="buffer"></param>
                /// <returns></returns>
                public static byte[] CRC32(byte[] buffer) => ComputeHash(buffer, CreateCRC32());

                /// <summary>
                /// 计算CRC32值
                /// </summary>
                /// <param name="inputStream"></param>
                /// <returns></returns>
                public static byte[] CRC32(Stream inputStream) => ComputeHash(inputStream, CreateCRC32());
            }
        }

        static TargetFramework? mTargetFramework;

        static TargetFramework TargetFramework
           => mTargetFramework ?? throw new NullReferenceException(nameof(mTargetFramework));

        public static void SetTargetFramework(Assembly assembly)
        {
            mTargetFramework = new TargetFramework(assembly);
        }

        public static void PrintTargetFramework()
            => TestContext.WriteLine($"CurrentTarget: {TargetFramework}");

        #region Stopwatch

        /// <summary>
        /// 运行函数，返回耗时，单位毫秒
        /// </summary>
        /// <param name="tag">显示标签</param>
        /// <param name="action">要运行的函数</param>
        /// <returns>运行函数耗时，单位毫秒</returns>
        public static long Run(string tag, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            TestContext.WriteLine($"{tag} 耗时：{stopwatch.ElapsedMilliseconds}毫秒");
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 运行函数，返回耗时，单位毫秒
        /// </summary>
        /// <param name="tag">显示标签</param>
        /// <param name="count">重复调用次数</param>
        /// <param name="action">要运行的函数</param>
        /// <returns>运行函数耗时，单位毫秒</returns>
        public static long Run(string tag, int count, Action action) => Run($"{tag} 次数：{count}", () =>
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        });

        /// <summary>
        /// 重复调用次数
        /// </summary>
        public static class Count
        {
            /// <summary>
            /// 中等
            /// </summary>
            public const int Middle = 4500;

            /// <summary>
            /// 大号
            /// </summary>
            public const int Large = 6500;

            /// <summary>
            /// 一万
            /// </summary>
            public const int _1W = 10000;

            /// <summary>
            /// 两万
            /// </summary>
            public const int _2W = 20000;

            /// <summary>
            /// 五万
            /// </summary>
            public const int _5W = 50000;

            /// <summary>
            /// 七万
            /// </summary>
            public const int _7W = 70000;

            /// <summary>
            /// 十万
            /// </summary>
            public const int _10W = 100000;
        }

        #endregion

        public static void Print<T>(IEnumerable<T>? list, string? prefix = null)
        {
            string str;
            if (list == null)
            {
                str = "null";
            }
            else
            {
                var array = list.Select(x => x?.ToString());
                str = string.Join(", ", array);
            }
            TestContext.WriteLine(string.IsNullOrEmpty(prefix) ? str : prefix + str);
        }

        public static void PrintJson<T>(T obj, string? prefix = null)
        {
            var str = Serializable2.S(obj);
            TestContext.WriteLine(string.IsNullOrEmpty(prefix) ? str : prefix + str);
        }

        public static void Equals<T>(this IReadOnlyList<IReadOnlyList<T>> list, bool print = false) where T : IEquatable<T>
        {
            for (int i = 0; i < list[0].Count; i++)
            {
                var array = list.Select(x => x[i]).Distinct().ToArray();
                var isSinge = array.Length == 1;
                if (!isSinge && print)
                {
                    for (int j = 0; j < array.Length; j++)
                    {
                        var item = list.FirstOrDefault(x => x[i].Equals(array[j]));
                        Print(item);
                    }
                }
                if (!isSinge)
                {
                    TestContext.WriteLine($"Equals, i: {i}, isSinge: {isSinge}.");
                }
                Assert.IsTrue(isSinge);
                if (!isSinge)
                {
                    return;
                }
            }
            if (print) Print(list[0]);
        }
    }
}

namespace System.Collections.Generic
{
    public sealed class DictionaryCanRepeatKey<TKey, TValue> : IReadOnlyList<(TKey Key, TValue Value)>
    {
        readonly IReadOnlyList<(TKey Key, TValue Value)> value;

        public DictionaryCanRepeatKey(IReadOnlyList<(TKey Key, TValue Value)> value) => this.value = value;

        public IReadOnlyList<TKey> Keys => value.Select(x => x.Key).ToArray();

        public IReadOnlyList<TValue> Values => value.Select(x => x.Value).ToArray();

        public (TKey Key, TValue Value) this[int index] => value[index];

        public int Count => value.Count;

        public IEnumerator<(TKey Key, TValue Value)> GetEnumerator()
        {
            return value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)value).GetEnumerator();
        }
    }
}