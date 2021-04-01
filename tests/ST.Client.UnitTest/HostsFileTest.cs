using Moq;
using NUnit.Framework;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Application
{
    [TestFixture]
    public class HostsFileTest
    {
        static void Test(string[] hosts, Action<IHostsFileService> action)
            => Test(hosts, (s, _) => action(s));

        static void Test(string[] hosts, Action<IHostsFileService, IDesktopPlatformService> action)
        {
            var tempFileName = Path.GetTempFileName();
            IOPath.FileIfExistsItDelete(tempFileName);
            File.WriteAllLines(tempFileName, hosts);
            var mock_dps = new Mock<IDesktopPlatformService>();
            mock_dps.Setup(x => x.HostsFilePath).Returns(tempFileName);
            IDesktopPlatformService dps = mock_dps.Object;
            IHostsFileService s = new HostsFileServiceImpl(dps);
            action(s, dps);
            File.Delete(tempFileName);
        }

        static bool TestEquals(List<(string ip, string domain)> left, List<(string ip, string domain)> right)
        {
            if (left.Count != right.Count) return false;
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

        static readonly string[] hosts_ = new[]
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

        static IEnumerable<(string ip, string domain)> GetHosts(string[] hosts)
        {
            var query = from m in hosts
                        where !string.IsNullOrWhiteSpace(m)
                        let array = m.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        where array.Length >= 2 && !array[0].StartsWith('#')
                        select (ip: array[0], domain: array[1]);
            return query;
        }

        [Test]
        public void ReadHostsAllLines()
        {
            var hosts = hosts_;
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

        [Test]
        public void UpdateHosts1()
        {
            const string ip = "127.0.0.2";
            const string domain = "abcdefg.steampp.net";
            var hosts = hosts_;
            void Action(IHostsFileService x)
            {
                var values_ = x.UpdateHosts(ip, domain);
                Assert.IsTrue(values_.ResultType == OperationResultType.Success, values_.Message);

                var values = x.ReadHostsAllLines();
                Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                var query = GetHosts(hosts);
                var values2 = query.ToList();
                values2.Add((ip, domain));
                var result = TestEquals(values.AppendData, values2);
                Assert.IsTrue(result);
            }
            Test(hosts, (s, dps) =>
            {
                Action(s);
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
            Test(hosts2.ToArray(), (s, dps) =>
            {
                Action(s);
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

        [Test]
        public void UpdateHosts2()
        {
            (string ip, string domain)[] datas = new[]
            {
                ("127.0.0.2", "qwert.steampp.net"),
                ("127.0.0.3", "zxcvb.steampp.net"),
            };
            var hosts = hosts_;
            void Action(IHostsFileService x)
            {
                var values_ = x.UpdateHosts(datas);
                Assert.IsTrue(values_.ResultType == OperationResultType.Success, values_.Message);

                var values = x.ReadHostsAllLines();
                Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                var query = from m in hosts where !string.IsNullOrWhiteSpace(m) let array = m.Split(' ', StringSplitOptions.RemoveEmptyEntries) where array.Length >= 2 select (ip: array[0], domain: array[1]);
                var values2 = query.ToList();
                values2.AddRange(datas);
                var result = TestEquals(values.AppendData, values2);
                Assert.IsTrue(result);
            }
            Test(hosts, (s, dps) =>
            {
                Action(s);
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
            Test(hosts2.ToArray(), (s, dps) =>
            {
                Action(s);
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

        [Test]
        public void RemoveHosts()
        {
            var hosts = hosts_;
            (string ip, string domain)[] datas = new[]
            {
                ("127.1.0.2", "1qwert.steampp.net"),
                ("127.1.0.3", "1zxcvb.steampp.net"),
            };
            var datas2 = datas.Select(x => $"{x.ip} {x.domain}");
            hosts = hosts.Concat(datas2).ToArray();
            Test(hosts, (s, dps) =>
            {
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
            });

            var hosts2 = new List<string>(hosts_)
            {
                HostsFileServiceImpl.MarkStart
            };
            hosts2.AddRange(datas2);
            hosts2.Add(HostsFileServiceImpl.MarkEnd);
            Test(hosts2.ToArray(), (s, dps) =>
            {
                var (ip, domain) = datas.First();
                var values = s.RemoveHosts(domain);
                Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                var filePath = dps.HostsFilePath;
                var lines = File.ReadAllLines(filePath).ToList();
                var index = lines.FindIndex(x => x.Contains($"{ip} {domain}"));
                Assert.IsTrue(index == -1, "index");
            });

            var hosts3 = new List<string>(hosts_)
            {
                HostsFileServiceImpl.MarkStart
            };
            hosts3.AddRange(datas2);
            hosts3.Add(HostsFileServiceImpl.MarkEnd);
            hosts3.Add(HostsFileServiceImpl.BackupMarkStart);
            var first_data = datas.First();
            var first_data_value = $"{first_data.ip}3 {first_data.domain}";
            hosts3.Add(first_data_value);
            hosts3.Add(HostsFileServiceImpl.BackupMarkEnd);
            Test(hosts3.ToArray(), (s, dps) =>
            {
                var (ip, domain) = datas.First();
                var values = s.RemoveHosts(domain);
                Assert.IsTrue(values.ResultType == OperationResultType.Success, values.Message);
                var filePath = dps.HostsFilePath;
                var lines = File.ReadAllLines(filePath).ToList();
                var index = lines.FindIndex(x => x.Contains($"{ip} {domain}"));
                Assert.IsTrue(index == -1, "index");
                var index2 = lines.FindIndex(x => x == first_data_value);
                Assert.IsTrue(index >= 0, "index2");
                // ...
            });
        }

        //[Test]
        //public void RemoveHostsByTag()
        //{
        //}
    }
}