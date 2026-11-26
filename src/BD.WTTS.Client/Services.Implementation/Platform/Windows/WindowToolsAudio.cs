#if WINDOWS
using static BD.WTTS.Services.Implementation.WindowToolsNative;

namespace BD.WTTS.Services.Implementation;

// One Core Audio enumeration per foreground update. Store session IDs and mute
// states rather than carrying apartment-bound COM pointers across event threads.
static unsafe partial class WindowToolsAudio
{
    static readonly Guid DeviceEnumeratorClass = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
    static readonly Guid DeviceEnumeratorIid = new("A95664D2-9614-4F35-A746-DE8DB63617E6");
    static readonly Guid SessionManagerIid = new("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
    static readonly Guid SessionControlIid = new("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D");
    static readonly Guid SimpleVolumeIid = new("87CE5498-68D6-44E5-9215-6DA47EF883D8");

    internal static void SetProcessMute(uint process, bool mute, Dictionary<string, bool> originalStates)
    {
        using var apartment = new ComScope();
        if (!apartment.IsReady)
            return;

        nint enumerator = 0, endpoints = 0;
        try
        {
            if (CoCreateInstance(in DeviceEnumeratorClass, 0, 1, in DeviceEnumeratorIid, out enumerator) < 0)
                return;
            // IMMDeviceEnumerator::EnumAudioEndpoints(eRender, DEVICE_STATE_ACTIVE)
            var enumerate = (delegate* unmanaged[Stdcall]<nint, int, uint, nint*, int>)Slot(enumerator, 3);
            if (enumerate(enumerator, 0, 1, &endpoints) < 0)
                return;

            var item = (delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)Slot(endpoints, 4);
            for (uint i = 0, count = Count(endpoints); i < count; ++i)
            {
                nint endpoint = 0;
                try
                {
                    if (item(endpoints, i, &endpoint) >= 0)
                        UpdateEndpoint(endpoint, process, mute, originalStates);
                }
                finally
                {
                    Release(endpoint);
                }
            }
        }
        finally
        {
            Release(endpoints);
            Release(enumerator);
        }
    }

    static void UpdateEndpoint(nint endpoint, uint process, bool mute, Dictionary<string, bool> originalStates)
    {
        nint manager = 0, sessions = 0;
        try
        {
            var iid = SessionManagerIid;
            // IMMDevice::Activate(IAudioSessionManager2), then GetSessionEnumerator.
            var activate = (delegate* unmanaged[Stdcall]<nint, Guid*, uint, nint, nint*, int>)Slot(endpoint, 3);
            if (activate(endpoint, &iid, 1, 0, &manager) < 0)
                return;
            var enumerate = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Slot(manager, 5);
            if (enumerate(manager, &sessions) < 0)
                return;

            var item = (delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)Slot(sessions, 4);
            for (uint i = 0, count = Count(sessions); i < count; ++i)
            {
                nint session = 0;
                try
                {
                    if (item(sessions, i, &session) >= 0)
                        UpdateSession(session, process, mute, originalStates);
                }
                finally
                {
                    Release(session);
                }
            }
        }
        finally
        {
            Release(sessions);
            Release(manager);
        }
    }

    static void UpdateSession(nint session, uint process, bool mute, Dictionary<string, bool> originals)
    {
        nint control = 0, volume = 0, identifier = 0;
        try
        {
            if (Marshal.QueryInterface(session, in SessionControlIid, out control) < 0)
                return;
            var getProcess = (delegate* unmanaged[Stdcall]<nint, uint*, int>)Slot(control, 14);
            var getInstanceId = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Slot(control, 13);
            uint pid = 0;
            // S_OK means this session belongs to one process. Multi-process sessions are left alone.
            if (getProcess(control, &pid) != 0 || pid != process || getInstanceId(control, &identifier) < 0 ||
                Marshal.PtrToStringUni(identifier) is not { } id)
                return;
            if (!mute && !originals.ContainsKey(id))
                return;
            if (Marshal.QueryInterface(session, in SimpleVolumeIid, out volume) < 0)
                return;

            var setMute = (delegate* unmanaged[Stdcall]<nint, int, nint, int>)Slot(volume, 5);
            if (mute)
            {
                int previous = 0;
                var getMute = (delegate* unmanaged[Stdcall]<nint, int*, int>)Slot(volume, 6);
                if (getMute(volume, &previous) >= 0 && setMute(volume, 1, 0) >= 0)
                    originals.TryAdd(id, previous != 0);
            }
            else if (setMute(volume, originals[id] ? 1 : 0, 0) >= 0)
            {
                originals.Remove(id);
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem(identifier);
            Release(volume);
            Release(control);
        }
    }

    // IMMDeviceCollection and IAudioSessionEnumerator both put GetCount at IUnknown + 0.
    static uint Count(nint collection)
    {
        var getCount = (delegate* unmanaged[Stdcall]<nint, uint*, int>)Slot(collection, 3);
        uint count = 0;
        return getCount(collection, &count) >= 0 ? count : 0;
    }

    static nint Slot(nint instance, int index) => (*(nint**)instance)[index];

    static void Release(nint instance)
    {
        if (instance != 0)
            Marshal.Release(instance);
    }

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid clsid, nint outer, uint context, in Guid iid, out nint instance);
}
#endif
