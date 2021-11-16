using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public class LocalDlssDll : IComparable<LocalDlssDll>
    {
        public string Filename { get; }

        public string Version { get; }

        public ulong VersionNumber { get; }

        public string SHA1Hash { get; }

        public string MD5Hash { get; }

        public LocalDlssDll(string filename)
        {
            Filename = filename;

            var versionInfo = FileVersionInfo.GetVersionInfo(filename);

            Version = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";
            VersionNumber = ((ulong)versionInfo.FileMajorPart << 48) +
                         ((ulong)versionInfo.FileMinorPart << 32) +
                         ((ulong)versionInfo.FileBuildPart << 16) +
                         ((ulong)versionInfo.FilePrivatePart);

            using (var stream = File.OpenRead(filename))
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(stream);
                    MD5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }

                stream.Position = 0;

                using (var sha1 = SHA1.Create())
                {
                    var hash = sha1.ComputeHash(stream);
                    SHA1Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        public override string ToString()
        {
            return Version;
        }

        public int CompareTo(LocalDlssDll? other)
        {
            if (other == null)
            {
                return -1;
            }

            return other.VersionNumber.CompareTo(VersionNumber);
        }
    }
}
