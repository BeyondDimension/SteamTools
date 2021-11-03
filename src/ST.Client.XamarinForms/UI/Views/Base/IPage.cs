using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Views
{
    public interface IPage
    {
#pragma warning disable IDE1006 // 命名样式
        Page @this { get; }
#pragma warning restore IDE1006 // 命名样式
    }
}
