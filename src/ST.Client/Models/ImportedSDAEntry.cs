using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public class ImportedSDAEntry
    {
        public const int PBKDF2_ITERATIONS = 50000;
        public const int SALT_LENGTH = 8;
        public const int KEY_SIZE_BYTES = 32;
        public const int IV_LENGTH = 16;

        public string? Username;
        public string? SteamId;
        public string? json;

        public override string ToString()
        {
            return Username + " (" + SteamId + ")";
        }
    }
}
