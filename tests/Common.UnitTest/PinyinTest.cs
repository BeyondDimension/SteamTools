using NUnit.Framework;

namespace System
{
    [TestFixture]
    public class PinyinTest
    {
        /// <summary>
        /// 完全的中文字符串获取拼音测试
        /// </summary>
        [Test]
        public void Full()
        {
            const string testStr = "长沙";
            var pinyinStr = "CHANG" + Pinyin.SeparatorVerticalBar + "SHA";
            // testStr 值如果有多音字可能导致测试不通过
            foreach (var item in testStr)
            {
                Assert.IsTrue(Pinyin.IsChinese(item));
            }
            var pinyinStr2 = Pinyin.GetPinyin(testStr, PinyinFormat.UpperVerticalBar);
            Assert.IsTrue(pinyinStr == pinyinStr2);
        }

        /// <summary>
        /// 混合的中文字符串获取拼音测试
        /// </summary>
        [Test]
        public void Blend()
        {
            const string testStr1 = "4S店";
            const string testStr2 = "学习ing";
            BlendCore(testStr1, testStr2);
            static void BlendCore(params string[] strs)
            {
                foreach (var item in strs)
                {
                    var pinyinStr = Pinyin.GetPinyin(item, PinyinFormat.UpperVerticalBar);
                    TestContext.WriteLine(pinyinStr);
                }
            }
        }

        /// <summary>
        /// 〇是一个汉字，líng同“零”
        /// </summary>
        [Test]
        public void CapitalZero()
        {
            // https://baike.baidu.com/item/%E3%80%87/457046
            const int capitalZero = 12295;
            const string capitalZeroPinyin = "LING";
            Assert.IsTrue(Pinyin.IsChinese((char)capitalZero));
            var pinyinStr = Pinyin.GetPinyin(((char)capitalZero).ToString(), PinyinFormat.UpperVerticalBar);
            Assert.IsTrue(pinyinStr == capitalZeroPinyin);
        }

        [Test]
        public void Info()
        {
            var is_a = Pinyin.IsChinese('A');
            TestContext.WriteLine($"A IsChinese: {is_a}");
            Assert.IsFalse(is_a);
            var is_p = Pinyin.IsChinese('拼');
            TestContext.WriteLine($"拼 IsChinese: {is_p}");
            Assert.IsTrue(is_p);
            var s_py = Pinyin.GetAlphabetSort("拼音");
            TestContext.WriteLine($"拼音 GetAlphabetSort: {s_py}");
            var s_ts = Pinyin.GetAlphabetSort("测试");
            TestContext.WriteLine($"测试 GetAlphabetSort: {s_ts}");
        }
    }
}