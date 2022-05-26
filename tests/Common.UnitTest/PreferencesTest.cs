using NUnit.Framework;
using System.Application.Services;

namespace System;

[TestFixture]
public class PreferencesTest
{
    [Test]
    public void All()
    {
        var s = IPreferencesPlatformService.Instance;
        const string key1 = "key1";
        const string key2 = "key2";
        const int value1 = 1;
        const int value2 = 2;
        s.PlatformSet(key1, key1, null);
        s.PlatformSet(key1, key1 + key2, key1);

        Assert.IsTrue(s.PlatformContainsKey(key1, null));
        Assert.IsTrue(s.PlatformContainsKey(key1, key1));

        Assert.IsTrue(s.PlatformGet<string>(key1, default, null) == key1);
        Assert.IsTrue(s.PlatformGet<string>(key1, default, key1) == key1 + key2);

        s.PlatformRemove(key1, null);
        Assert.IsTrue(!s.PlatformContainsKey(key1, null));

        s.PlatformRemove(key1, key1);
        Assert.IsTrue(!s.PlatformContainsKey(key1, key1));

        s.PlatformSet(key2, value1, null);
        s.PlatformSet(key2, value1 + value2, key2);

        Assert.IsTrue(s.PlatformContainsKey(key2, null));
        Assert.IsTrue(s.PlatformContainsKey(key2, key2));

        Assert.IsTrue(s.PlatformGet<int>(key2, default, null) == value1);
        Assert.IsTrue(s.PlatformGet<int>(key2, default, key2) == value1 + value2);

        s.PlatformRemove(key2, null);
        Assert.IsTrue(!s.PlatformContainsKey(key2, null));

        s.PlatformRemove(key2, key2);
        Assert.IsTrue(!s.PlatformContainsKey(key2, key2));

        s.PlatformSet(key1, value1, null);

        s.PlatformClear(null);

        Assert.IsTrue(!s.PlatformContainsKey(key1, null));

        s.PlatformSet(key1, value1 + value2, key1);

        s.PlatformClear(key1);

        Assert.IsTrue(!s.PlatformContainsKey(key1, key1));
    }
}
