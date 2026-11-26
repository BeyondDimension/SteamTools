#if WINDOWS
using System.Threading.Channels;
using static BD.WTTS.Services.Implementation.WindowToolsNative;

namespace BD.WTTS.Services.Implementation;

sealed class WindowToolsHooks : IDisposable
{
    internal const uint ForegroundChanged = 3, WindowDestroyed = 0x8001;

    readonly Action<uint, nint> windowEvent;
    readonly Action<Input> forward;
    readonly HookCallback mouseCallback, keyboardCallback;
    readonly EventCallback eventCallback;
    readonly uint threadId;
    readonly Channel<Input> input = Channel.CreateBounded<Input>(new BoundedChannelOptions(128)
    {
        SingleReader = true,
        SingleWriter = true,
        FullMode = BoundedChannelFullMode.DropOldest,
    });

    nint mouseHook, keyboardHook, foregroundHook, destroyHook;
    nint[] keyboardDesktopHosts = [];
    nint[] trackedWindows = [];
    int requestedHooks;
    int lastMouseX, lastMouseY;
    uint lastMoveTime;

    internal readonly record struct Input(uint Message, Point Point, uint Data, uint Flags, uint Time, uint ScanCode, uint MouseKeys, nint Foreground)
    {
        internal bool IsMouse => Message >= 0x200;
    }

    public WindowToolsHooks(Action<uint, nint> windowEvent, Action<Input> forward)
    {
        this.windowEvent = windowEvent;
        this.forward = forward;
        mouseCallback = Mouse;
        keyboardCallback = Keyboard;
        eventCallback = WindowEvent;

        var ready = new TaskCompletionSource<uint>();
        new Thread(() => PumpMessages(ready)) { IsBackground = true, Name = nameof(WindowToolsHooks) }.Start();
        threadId = ready.Task.GetAwaiter().GetResult();
        _ = Task.Run(ForwardInputAsync);
    }

    internal void Configure(bool mouse, nint[] desktopHosts, nint[] watchedWindows, bool foreground)
    {
        Volatile.Write(ref keyboardDesktopHosts, desktopHosts);
        Volatile.Write(ref trackedWindows, watchedWindows);
        var requested = (mouse ? 1 : 0) | (desktopHosts.Length != 0 ? 2 : 0) | (foreground ? 4 : 0) |
            (watchedWindows.Length != 0 ? 8 : 0);
        if (Interlocked.Exchange(ref requestedHooks, requested) == requested)
            return;
        PostThreadMessage(threadId, WmConfigure, 0, 0);
    }

    void PumpMessages(TaskCompletionSource<uint> ready)
    {
        PeekMessage(out _, 0, 0, 0, 0); // Create the thread's native message queue.
        ready.SetResult(GetCurrentThreadId());
        try
        {
            while (GetMessage(out var message, 0, 0, 0) > 0)
            {
                if (message.Id != WmConfigure)
                    continue;

                var requested = Volatile.Read(ref requestedHooks);
                var module = GetModuleHandle(null);
                SetInputHook(ref mouseHook, (requested & 1) != 0, 14, mouseCallback, module);
                SetInputHook(ref keyboardHook, (requested & 2) != 0, 13, keyboardCallback, module);
                SetEventHook(ref foregroundHook, (requested & 4) != 0, ForegroundChanged);
                SetEventHook(ref destroyHook, (requested & 8) != 0, WindowDestroyed);
            }
        }
        finally
        {
            SetInputHook(ref mouseHook, false, 14, mouseCallback, 0);
            SetInputHook(ref keyboardHook, false, 13, keyboardCallback, 0);
            SetEventHook(ref foregroundHook, false, ForegroundChanged);
            SetEventHook(ref destroyHook, false, WindowDestroyed);
        }
    }

    static void SetInputHook(ref nint hook, bool enabled, int kind, HookCallback callback, nint module)
    {
        if (enabled && hook == 0)
            hook = SetWindowsHookEx(kind, callback, module, 0);
        else if (!enabled && hook != 0)
        {
            UnhookWindowsHookEx(hook);
            hook = 0;
        }
    }

    void SetEventHook(ref nint hook, bool enabled, uint @event)
    {
        if (enabled && hook == 0)
            hook = SetWinEventHook(@event, @event, 0, eventCallback, 0, 0, 0);
        else if (!enabled && hook != 0)
        {
            UnhookWinEvent(hook);
            hook = 0;
        }
    }

    unsafe nint Mouse(int code, nuint message, nint data)
    {
        // The low-level callback runs on a dedicated message pump. Queue only value types.
        // WindowFromPoint, accessibility, and cross-process calls run on a separate worker.
        if (code == 0 && (Volatile.Read(ref requestedHooks) & 1) != 0)
        {
            var mouse = *(MouseData*)data;
            if ((mouse.Flags & 1) == 0 && (message != 0x200 || ShouldQueueMove(mouse)))
                input.Writer.TryWrite(new((uint)message, mouse.Point, mouse.Data, mouse.Flags, mouse.Time, 0,
                    MouseKeys((uint)message, mouse.Data), 0));
        }

        return CallNextHookEx(0, code, message, data);
    }

    bool ShouldQueueMove(MouseData mouse)
    {
        var elapsed = unchecked(mouse.Time - lastMoveTime);
        if (elapsed < 12 || (mouse.Point.X == lastMouseX && mouse.Point.Y == lastMouseY))
            return false;

        lastMoveTime = mouse.Time;
        lastMouseX = mouse.Point.X;
        lastMouseY = mouse.Point.Y;
        return true;
    }

    static uint MouseKeys(uint message, uint data)
    {
        var keys = KeyFlag(1, 1) | KeyFlag(2, 2) | KeyFlag(0x10, 4) | KeyFlag(0x11, 8) |
            KeyFlag(4, 0x10) | KeyFlag(5, 0x20) | KeyFlag(6, 0x40);
        var flag = message switch
        {
            0x201 or 0x202 => 1u,
            0x204 or 0x205 => 2u,
            0x207 or 0x208 => 0x10u,
            0x20B or 0x20C => (data >> 16) == 1 ? 0x20u : 0x40u,
            _ => 0u,
        };
        return message is 0x202 or 0x205 or 0x208 or 0x20C ? keys & ~flag : keys | flag;
    }

    static uint KeyFlag(int key, uint flag) => GetAsyncKeyState(key) < 0 ? flag : 0;

    unsafe nint Keyboard(int code, nuint message, nint data)
    {
        if (code == 0 && (Volatile.Read(ref requestedHooks) & 2) != 0 && message is 0x100 or 0x101)
        {
            var foreground = GetForegroundWindow();
            if (Array.IndexOf(Volatile.Read(ref keyboardDesktopHosts), foreground) >= 0)
            {
                var key = *(KeyData*)data;
                if ((key.Flags & 0x10) == 0 && key.Key is not (0x5B or 0x5C) &&
                    GetAsyncKeyState(0x5B) >= 0 && GetAsyncKeyState(0x5C) >= 0)
                    input.Writer.TryWrite(new((uint)message, default, key.Key, key.Flags, key.Time, key.ScanCode, 0, foreground));
            }
        }

        return CallNextHookEx(0, code, message, data);
    }

    void WindowEvent(nint hook, uint @event, nint window, int objectId, int childId, uint thread, uint time)
    {
        if (window != 0 && objectId == 0 && childId == 0 &&
            (@event == ForegroundChanged || Array.IndexOf(Volatile.Read(ref trackedWindows), window) >= 0))
            ThreadPool.QueueUserWorkItem(state => windowEvent(state.Event, state.Window), (Event: @event, Window: window), false);
    }

    async Task ForwardInputAsync()
    {
        await foreach (var item in input.Reader.ReadAllAsync())
            forward(item);
    }

    public void Dispose()
    {
        Volatile.Write(ref requestedHooks, 0);
        input.Writer.TryComplete();
        PostThreadMessage(threadId, WmQuit, 0, 0);
    }
}
#endif
