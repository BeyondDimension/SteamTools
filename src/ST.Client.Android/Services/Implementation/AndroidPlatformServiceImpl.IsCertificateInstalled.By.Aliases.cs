using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Java.Security;
using JX509Certificate = Java.Security.Cert.X509Certificate;

namespace System.Application.Services.Implementation
{
    partial class AndroidPlatformServiceImpl
    {
        static IEnumerable<KeyValuePair<string, string>> Deserialize(string subject)
        {
            var items = subject.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                var array = item.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (array.Length == 2)
                {
                    yield return new KeyValuePair<string, string>(array[0], array[1]);
                }
            }
        }

        /// <summary>
        /// 检查该 X509 证书是否在 CA 存储区存在
        /// <para> <see langword="true"/> 表示在 System 中</para>
        /// <para> <see langword="false"/> 表示在 User 中</para>
        /// <para> <see langword="null"/> 表示 不存在</para>
        /// </summary>
        /// <param name="certificate2"></param>
        /// <returns></returns>
        internal static bool? IsCertificateInstalled(X509Certificate2 certificate2)
        {
            var subjectCN = Deserialize(certificate2.Subject).FirstOrDefault(x => x.Key == "CN").Value;

            using var androidCAStore = KeyStore.GetInstance("AndroidCAStore");
            if (androidCAStore == null) throw new ArgumentNullException(nameof(androidCAStore));
            androidCAStore.Load(null);

            // com.adguard.android.service.p.b()
            // com.adguard.android.service.q.u()
            using var aliases = androidCAStore.Aliases();
            if (aliases == null) throw new ArgumentNullException(nameof(aliases));

            List<string> aliaseMatchCaches = new();

            while (aliases.HasMoreElements) // 遍历所有 CA 存储区的证书
            {
                var nextElement = aliases.NextElement()?.ToString();
                if (nextElement == null) continue;
                var certificate = androidCAStore.GetCertificate(nextElement);
                if (certificate is not JX509Certificate x509Certificate) continue;
                var name = x509Certificate.SubjectDN?.Name; // 根据 CN 名字字符串匹配
                if (name == null ||
                    !name.Contains(subjectCN, StringComparison.OrdinalIgnoreCase)) continue;
                var encoded = x509Certificate.GetEncoded(); // 二进制数据比较相等
                if (encoded == null || !encoded.SequenceEqual(certificate2.RawData)) continue;
                if (nextElement.StartsWith("system")) return true; // 如果证书已导入 System，则直接返回
                aliaseMatchCaches.Add(nextElement);
            }

            if (aliaseMatchCaches.Any(x => x.StartsWith("user"))) return false; // 已导入到 User 中

            return null;
        }

        bool IPlatformService.IsCertificateInstalled(X509Certificate2 certificate2)
        {
            var value = IsCertificateInstalled(certificate2);
            return value.HasValue;
        }
    }
}