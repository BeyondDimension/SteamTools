using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public interface IPageViewModel : IViewModelBase
    {
        string Title { get; set; }
    }
}
