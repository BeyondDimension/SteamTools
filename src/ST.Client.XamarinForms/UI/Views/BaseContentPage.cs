using ReactiveUI.XamForms;
using System;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace System.Application.UI.Views
{
    public abstract class BaseContentPage<TViewModel> : ReactiveContentPage<TViewModel> where TViewModel : ViewModelBase
    {
        public virtual string TitlePropertyPath => nameof(TabItemViewModelBase.Name);

        public BaseContentPage()
        {
            this.SetBinding(TitleProperty, TitlePropertyPath, BindingMode.OneWay);
        }
    }
}
