using Java.Lang.Ref;
using Libv2ray;
using JObject = Java.Lang.Object;

namespace System.Application.Services.V2Ray
{
    static class V2RayServiceManager
    {
        static readonly V2RayPoint v2rayPoint = Libv2ray.Libv2ray.NewV2RayPoint(new V2RayCallback());

        static SoftReference? mServiceControl;

        public static SoftReference? ServiceControl
        {
            set
            {
                mServiceControl = value;
                if (value?.Get() is IServiceControl serviceControl)
                {
                    var context = serviceControl.Service?.ApplicationContext;
                }
            }
            get
            {
                return mServiceControl;
            }
        }

        sealed class V2RayCallback : JObject, IV2RayVPNServiceSupportsSet
        {
            public long OnEmitStatus(long p0, string p1)
            {
                throw new NotImplementedException();
            }

            public long Prepare()
            {
                throw new NotImplementedException();
            }

            public long Protect(long p0)
            {
                throw new NotImplementedException();
            }

            public long Setup(string p0)
            {
                throw new NotImplementedException();
            }

            public long Shutdown()
            {
                throw new NotImplementedException();
            }
        }

        public static void StartV2rayPoint()
        {

        }
    }
}
