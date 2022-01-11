using System.Application.Services;
using System.Security.Cryptography;
using System.Linq;
using System.Application.Models;
using _Constants = System.Common.Constants;

namespace System.Application
{
    public static partial class DeviceIdHelper
    {
        static Guid GetGuid(string key)
        {
            var value = Preferences2.Get(key, null);
            if (!(!string.IsNullOrWhiteSpace(value) && ShortGuid.TryParse(value, out Guid guid)))
            {
                guid = Guid.NewGuid();
                Preferences2.Set(key, guid.ToStringS());
            }
            return guid;
        }

        static string Get(string key)
        {
            var value = Preferences2.Get(key, null);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Random2.GenerateRandomString(DeviceIdRLength, _Constants.DigitsLetters);
                Preferences2.Set(key, value);
            }
            return value;
        }

        static Guid DeviceIdG => GetGuid("KEY_DEVICE_ID_G");

        static string DeviceIdR => Get("KEY_DEVICE_ID_R");

        static string DeviceIdN
        {
            get
            {
                var s = IPlatformService.Instance;
                (var key, var iv) = s.MachineSecretKey;
                var data = key.FirstOrDefault() % 2 == 0 ? key.Concat(iv) : iv.Concat(key);
                return Hashs.String.SHA256(data.ToArray(), isLower: false);
            }
        }

        public static void SetDeviceId(this ActiveUserRecordDTO value)
        {
            var deviceIdG = DeviceIdG;
            var deviceIdR = DeviceIdR;
            var deviceIdN = DeviceIdN;

            value.DeviceIdG = deviceIdG;
            value.DeviceIdR = deviceIdR;
            value.DeviceIdN = deviceIdN;
        }
    }
}
