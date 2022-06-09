using ReactiveUI;
using SteamKit2;
using System;
using System.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FilePath = System.IO.Path;

namespace System.Application.Models
{
    public class SteamRemoteFile : ReactiveObject
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public string Name { get; private set; }

        public bool Exists { get; }

        public bool IsPersisted { get; }

        public long Size { get; }

        public DateTimeOffset Timestamp { get; }

        public ERemoteStoragePlatform SyncPlatforms { get; set; }

        public SteamRemoteFile(string name)
        {
            Name = name;
        }

        public SteamRemoteFile(string name, long length, bool exists, bool isPersisted, long timestamp)
        {
            Name = name;
            Size = length;
            Exists = exists;
            IsPersisted = isPersisted;
            Timestamp = timestamp.ToDateTimeOffsetS();
        }

        public int Read(byte[] buffer)
        {
            return ISteamworksLocalApiService.Instance.FileRead(Name, buffer);
        }

        public byte[] ReadAllBytes()
        {
            byte[] buffer = new byte[Size];
            int read = Read(buffer);
            if (read != buffer.Length) throw new IOException("Could not read entire file.");
            return buffer;
        }

        public bool WriteAllBytes(byte[] buffer)
        {
            return ISteamworksLocalApiService.Instance.FileWrite(Name, buffer);
        }

        public bool Forget()
        {
            return ISteamworksLocalApiService.Instance.FileForget(Name);
        }

        public bool Delete()
        {
            return ISteamworksLocalApiService.Instance.FileDelete(Name);
        }
    }
}
