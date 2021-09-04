using System;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace System.Application.UI.Views.Controls
{
    public class EmbedSample : NativeControlHost
    {
        IntPtr _handle;
        //public EmbedSample()
        //{
        //    this.AttachedToVisualTree += EmbedSample_AttachedToVisualTree; ;
        //}

        //private void EmbedSample_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
        //{
        //    if (UISettings.EnableDesktopBackground.Value)
        //    {
        //        DI.Get<ISystemWindowApiService>().SetDesktopBackgroundToWindow(
        //            this._handle, 600, 900);
        //    }
        //}

        IPlatformHandle CreateWin32(IPlatformHandle parent)
        {
            //var handle = DI.Get<ISystemWindowApiService>().CreateEmptyWindow(parent.Handle);
            _handle = parent.Handle;
            return new PlatformHandle(_handle, "HWND");
        }

        void DestroyWin32(IPlatformHandle handle)
        {
            //DI.Get<ISystemWindowApiService>().CreateEmptyWindow(handle.Handle, false);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return CreateWin32(parent);
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //    return CreateLinux(parent);
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //    return CreateOSX(parent);
            return base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                DestroyWin32(control);
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //    DestroyLinux(control);
            //else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //    DestroyOSX(control);
            //else
            base.DestroyNativeControlCore(control);
        }
    }
}
