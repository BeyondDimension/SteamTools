using Moq;
using NUnit.Framework;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using R = System.Application.Properties.Resources;

namespace System.Application
{
    [TestFixture]
    public class HostsFileTest
    {
        static readonly List<string> tempFileNames = new();

        public static void DeleteAllTempFileName()
        {
            foreach (var item in tempFileNames)
            {
                IOPath.FileIfExistsItDelete(item, notCreateDir: true);
            }
        }

        static void Test(string[] hosts, Action<IHostsFileService> action)
             => Test(hosts, (_, s, _) => action(s));

        static void Test(string[] hosts, Action<string[], IHostsFileService, IDesktopPlatformService> action)
        {
            var tempFileName = Path.GetTempFileName();
            tempFileNames.Add(tempFileName);
            IOPath.FileIfExistsItDelete(tempFileName);
            File.WriteAllLines(tempFileName, hosts);
            var mock_dps = new Mock<IDesktopPlatformService>();
            mock_dps.Setup(x => x.HostsFilePath).Returns(tempFileName);
            IDesktopPlatformService dps = mock_dps.Object;
            IHostsFileService s = new HostsFileServiceImpl(dps);
            action(hosts, s, dps);
            File.Delete(tempFileName);
            tempFileNames.Remove(tempFileName);
        }

        static bool TestEquals(IEnumerable<(string ip, string domain)> left, IEnumerable<(string ip, string domain)> right)
        {
            if (left.Count() != right.Count()) return false;
            foreach (var (ip, domain) in left)
            {
                var item = right.FirstOrDefault(x => x.domain == domain);
                if (item.ip != ip)
                {
                    return false;
                }
            }
            return true;
        }

        static readonly string[] hosts_;
        static readonly List<string[]> hosts_list;

        static HostsFileTest()
        {
            hosts_ = new[]
            {
                "127.0.0.1 steampp.net",
                "127.0.0.1 bbb.steampp.net",
                "127.0.0.1 ccc.steampp.net",
                "127.0.0.1 test.steampp.net",
                "",
                "127.0.0.1 steamcommunity.com #Steam++",
                "127.0.0.1 www.steamcommunity.com #Steam++",
                "127.0.0.1 steamcdn-a.akamaihd.net #Steam++",
                "127.0.0.1 raw.github.com #Steam++",
                "127.0.0.1 ddd.steampp.net",
                "# Steam++ End",
                "# xxx adas dsgdsa scxvcxzvcxzv",
                "# Copyright (c) 1993-2009 Microsoft Corp.",
                "# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.",
                "#",
                "#  ",
                "#	::1             localhost",
            };
            hosts_list = new()
            {
                hosts_,
                R.hosts_8F473F98.Split(Environment.NewLine),
            };
        }

        static IEnumerable<(string ip, string domain)> GetHosts(string[] hosts)
        {
            var query = from m in hosts
                        where !string.IsNullOrWhiteSpace(m)
                        let line_split_array = m.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        where !HostsFileServiceImpl.IsV1Format(line_split_array) &&
                        line_split_array.Length >= 2 &&
                        !line_split_array[0].StartsWith('#')
                        select (ip: line_split_array[0], domain: line_split_array[1], line: line_split_array[0] + ' ' + line_split_array[1]);
            var values = query.ToArray();
            var lines = query.Select(x => x.line).Distinct().ToArray();
            return lines.Select(x => values.Reverse().FirstOrDefault(y => y.line == x)).Select(x => (x.ip, x.domain)).ToArray();
        }

        [Test]
        public void ReadHostsAllLines()
        {
            foreach (var hosts in hosts_list)
            {
                Test(hosts, x =>
                {
                    var values = x.ReadHostsAllLines();
                    Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                    var query = GetHosts(hosts);
                    var values2 = query.ToList();
                    var result = TestEquals(values.AppendData, values2);
                    Assert.IsTrue(result);
                });
            }
        }

        [Test]
        public void SingleUpdateHosts()
        {
            const string ip = "127.0.0.2";
            const string domain = "abcdefg.steampp.net";

            foreach (var hosts in hosts_list)
            {
                void Action(string[] hosts, IHostsFileService x, string ip, string domain)
                {
                    var values_ = x.UpdateHosts(ip, domain);
                    Assert.IsTrue(values_.ResultType == OperationResultType.Success, values_.Message);

                    var values = x.ReadHostsAllLines();
                    Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                    var query = GetHosts(hosts);
                    var values2 = query.ToList();
                    values2.RemoveAll(x => x.domain == domain);
                    values2.Add((ip, domain));
                    var result = TestEquals(values.AppendData, values2);
                    Assert.IsTrue(result);
                }
                Test(hosts, (hosts, s, dps) =>
                {
                    Action(hosts, s, ip, domain);
                    var filePath = dps.HostsFilePath;
                    var lines = File.ReadAllLines(filePath).ToList();
                    var markSIndex = lines.FindIndex(x => x == HostsFileServiceImpl.MarkStart);
                    Assert.IsTrue(markSIndex >= 0, "markSIndex");
                    var markEIndex = lines.FindIndex(x => x == HostsFileServiceImpl.MarkEnd);
                    Assert.IsTrue(markEIndex >= 0, "markEIndex");
                    var index = lines.FindIndex(x => x == $"{ip} {domain}");
                    Assert.IsTrue(index > markSIndex, "index > markSIndex");
                    Assert.IsTrue(index < markEIndex, "index < markEIndex");
                });

                var test_line_value = $"{ip}3 {domain}";
                var hosts2 = new List<string>(hosts_)
            {
                test_line_value,
            };
                Test(hosts2.ToArray(), (hosts, s, dps) =>
                {
                    var test_line_value_array = test_line_value.Split(' ');
                    Action(hosts, s, test_line_value_array[0], test_line_value_array[1]);
                    var filePath = dps.HostsFilePath;
                    var lines = File.ReadAllLines(filePath).ToList();
                    var backMarkSIndex = lines.FindIndex(x => x == HostsFileServiceImpl.BackupMarkStart);
                    Assert.IsTrue(backMarkSIndex >= 0, "backMarkSIndex");
                    var backMarkEIndex = lines.FindIndex(x => x == HostsFileServiceImpl.BackupMarkEnd);
                    Assert.IsTrue(backMarkEIndex >= 0, "backMarkEIndex");
                    var backIndex = lines.FindIndex(x => x.EndsWith(test_line_value) && x.StartsWith('#'));
                    Assert.IsTrue(backIndex > backMarkSIndex, "backIndex > backMarkSIndex");
                    Assert.IsTrue(backIndex < backMarkEIndex, "backIndex < backMarkEIndex");
                });
            }
        }

        [Test]
        public void MultipleUpdateHosts()
        {
            (string ip, string domain)[] datas = new[]
            {
                ("127.0.0.2", "qwert.steampp.net"),
                ("127.0.0.3", "zxcvb.steampp.net"),
            };

            foreach (var hosts in hosts_list)
            {
                void Action(string[] hosts, IHostsFileService x, IEnumerable<(string ip, string domain)> datas)
                {
                    var values_ = x.UpdateHosts(datas);
                    Assert.IsTrue(values_.ResultType == OperationResultType.Success, values_.Message);

                    var values = x.ReadHostsAllLines();
                    Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                    var query = GetHosts(hosts);
                    var values2 = query.ToList();
                    values2.RemoveAll(x => datas.Select(x => x.domain).Contains(x.domain));
                    values2.AddRange(datas);
                    var result = TestEquals(values.AppendData, values2);
                    Assert.IsTrue(result);
                }
                Test(hosts, (hosts, s, dps) =>
                {
                    Action(hosts, s, datas);
                    var filePath = dps.HostsFilePath;
                    var lines = File.ReadAllLines(filePath).ToList();
                    var markSIndex = lines.FindIndex(x => x == HostsFileServiceImpl.MarkStart);
                    Assert.IsTrue(markSIndex >= 0, "markSIndex");
                    var markEIndex = lines.FindIndex(x => x == HostsFileServiceImpl.MarkEnd);
                    Assert.IsTrue(markEIndex >= 0, "markEIndex");
                    foreach (var (ip, domain) in datas)
                    {
                        var index = lines.FindIndex(x => x == $"{ip} {domain}");
                        Assert.IsTrue(index > markSIndex, "index > markSIndex");
                        Assert.IsTrue(index < markEIndex, "index < markEIndex");
                    }
                });

                var test_line_values = datas.Select(x => $"{x.ip}4 {x.domain}");
                var hosts2 = new List<string>(hosts_);
                hosts2.AddRange(test_line_values);
                Test(hosts2.ToArray(), (hosts, s, dps) =>
                {
                    Action(hosts, s,
                        from m in test_line_values
                        let test_line_values_array = m.Split(' ')
                        select (test_line_values_array[0], test_line_values_array[1]));
                    var filePath = dps.HostsFilePath;
                    var lines = File.ReadAllLines(filePath).ToList();
                    var backMarkSIndex = lines.FindIndex(x => x == HostsFileServiceImpl.BackupMarkStart);
                    Assert.IsTrue(backMarkSIndex >= 0, "backMarkSIndex");
                    var backMarkEIndex = lines.FindIndex(x => x == HostsFileServiceImpl.BackupMarkEnd);
                    Assert.IsTrue(backMarkEIndex >= 0, "backMarkEIndex");
                    foreach (var test_line_value in test_line_values)
                    {
                        var backIndex = lines.FindIndex(x => x.EndsWith(test_line_value) && x.StartsWith('#'));
                        Assert.IsTrue(backIndex > backMarkSIndex, "backIndex > backMarkSIndex");
                        Assert.IsTrue(backIndex < backMarkEIndex, "backIndex < backMarkEIndex");
                    }
                });
            }
        }

        [Test]
        public void RemoveHosts()
        {
            (string ip, string domain)[] datas = new[]
            {
                ("127.1.0.2", "1qwert.steampp.net"),
                ("127.1.0.3", "1zxcvb.steampp.net"),
            };

            foreach (var hosts_ in hosts_list)
            {
                var hosts = hosts_;
                var datas2 = datas.Select(x => $"{x.ip} {x.domain}");
                hosts = hosts.Concat(datas2).ToArray();
                Test(hosts, (hosts, s, dps) =>
                {
                    var str_value_0 = GetHosts(File.ReadAllLines(dps.HostsFilePath));
                    //var str_value_0_ = File.ReadAllText(dps.HostsFilePath);

                    var (ip, domain) = datas.First();
                    var values = s.RemoveHosts(domain);
                    Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                    var filePath = dps.HostsFilePath;
                    var lines = File.ReadAllLines(filePath).ToList();
                    var backMarkSIndex = lines.FindIndex(x => x == HostsFileServiceImpl.BackupMarkStart);
                    Assert.IsTrue(backMarkSIndex >= 0, "backMarkSIndex");
                    var backMarkEIndex = lines.FindIndex(x => x == HostsFileServiceImpl.BackupMarkEnd);
                    Assert.IsTrue(backMarkEIndex >= 0, "backMarkEIndex");
                    var backIndex = lines.FindIndex(x => x.EndsWith($"{ip} {domain}") && x.StartsWith('#'));
                    Assert.IsTrue(backIndex > backMarkSIndex, "backIndex > backMarkSIndex");
                    Assert.IsTrue(backIndex < backMarkEIndex, "backIndex < backMarkEIndex");

                    var str_value_1 = File.ReadAllText(dps.HostsFilePath);
                    TestContext.WriteLine(str_value_1);

                    var values2 = s.RemoveHosts(domain);
                    Assert.IsTrue(values2.ResultType == OperationResultType.Success, values2.Message);

                    var str_value_2 = GetHosts(File.ReadAllLines(dps.HostsFilePath));
                    //var str_value_2_ = File.ReadAllText(dps.HostsFilePath);
                    var isTrue = TestEquals(str_value_0, str_value_2);
                    Assert.IsTrue(isTrue, "str_value_0 == str_value_2");
                });
            }
        }

        [Test]
        public void RemoveHostsByTag()
        {
            (string ip, string domain)[] datas = new[]
            {
                ("127.1.0.2", "1qwert.steampp.net"),
                ("127.1.0.3", "1zxcvb.steampp.net"),
            };

            var hosts_list = new List<string[]>()
            {
                hosts_,
                R.hosts_8F473F98.Split(Environment.NewLine),
            };

            foreach (var hosts in hosts_list)
            {
                Test(hosts, (hosts, s, dps) =>
                {
                    var str_value_0 = GetHosts(File.ReadAllLines(dps.HostsFilePath));
                    //var str_value_0_ = File.ReadAllText(dps.HostsFilePath);

                    var values = s.UpdateHosts(datas);
                    Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);

                    var str_value_1 = File.ReadAllText(dps.HostsFilePath);
                    TestContext.WriteLine(str_value_1);

                    var values2 = s.RemoveHostsByTag();
                    Assert.IsTrue(values2.ResultType == OperationResultType.Success, values2.Message);

                    var str_value_2 = GetHosts(File.ReadAllLines(dps.HostsFilePath));
                    //var str_value_2_ = File.ReadAllText(dps.HostsFilePath);
                    var isTrue = TestEquals(str_value_0, str_value_2);
                    Assert.IsTrue(isTrue, "str_value_0 == str_value_2");
                });
            }
        }

        [Test]
        public void RemoveHostsByTag2()
        {
            (string ip, string domain)[] datas = new[]
            {
                ("127.1.0.2", "1qwert.steampp.net"),
                ("127.1.0.3", "1zxcvb.steampp.net"),
            };

            foreach (var hosts_ in hosts_list)
            {
                var hosts = hosts_;
                var datas2 = datas.Select(x => $"{x.ip} {x.domain}");
                hosts = hosts.Concat(datas2).ToArray();
                Test(hosts, (hosts, s, dps) =>
                {
                    var str_value_0 = GetHosts(File.ReadAllLines(dps.HostsFilePath));
                    //var str_value_0_ = File.ReadAllText(dps.HostsFilePath);

                    var values = s.UpdateHosts(datas.Select(x => (x.ip + "3", x.domain)));
                    Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);

                    var str_value_1 = File.ReadAllText(dps.HostsFilePath);
                    TestContext.WriteLine(str_value_1);

                    var values2 = s.RemoveHostsByTag();
                    Assert.IsTrue(values2.ResultType == OperationResultType.Success, values2.Message);

                    var str_value_2 = GetHosts(File.ReadAllLines(dps.HostsFilePath));
                    //var str_value_2_ = File.ReadAllText(dps.HostsFilePath);
                    var isTrue = TestEquals(str_value_0, str_value_2);
                    Assert.IsTrue(isTrue, "str_value_0 == str_value_2");
                });
            }
        }
    }
}