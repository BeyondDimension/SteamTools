using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class HandleWindowExtensions
    {
        public static bool IsHasProcessExits(this HandleWindow window)
        {
            if (window?.Process?.HasExited == false && window.Name != Process.GetCurrentProcess().ProcessName)
            {
                return false;
            }
            return true;
        }

        public static void Kill(this HandleWindow window)
        {
            if (!window.IsHasProcessExits())
            {
                window.Process?.Kill();
            }
        }
    }
}
