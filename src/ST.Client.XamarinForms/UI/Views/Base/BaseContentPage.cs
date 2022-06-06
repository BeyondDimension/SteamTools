using System;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !MAUI
using Xamarin.Forms;
using ReactiveUI.XamForms;
#else
using ReactiveUI.Maui;
#endif

namespace System.Application.UI.Views
{
    public abstract class BaseContentPage<TViewModel> : ReactiveContentPage<TViewModel>, IPage where TViewModel : ViewModelBase
    {
        public virtual string TitlePropertyPath
            => typeof(TViewModel).IsSubclassOf(typeof(TabItemViewModelBase)) ?
                nameof(TabItemViewModelBase.Name) :
                nameof(PageViewModel.Title);

        Page IPage.@this => this;

        public BaseContentPage()
        {
            this.SetBinding(TitleProperty, TitlePropertyPath, BindingMode.OneWay);
        }
    }
}
