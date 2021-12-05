using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public abstract partial class ItemViewModel : ViewModelBase
    {
        public abstract string Name { get; }

        #region IsSelected 変更通知

        private bool _IsSelected;

        public virtual bool IsSelected
        {
            get => _IsSelected;
            set => this.RaiseAndSetIfChanged(ref _IsSelected, value);
        }

        #endregion

        #region IsShowTab 変更通知

        private bool _IsShowTab = true;

        public virtual bool IsShowTab
        {
            get => _IsShowTab;
            set => this.RaiseAndSetIfChanged(ref _IsShowTab, value);
        }

        #endregion

        #region Resource Key 图标

        const string DefaultIconPath = "avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/AppResources/Icon/{0}.png";
        public virtual string? IconSource => string.Format(DefaultIconPath, IconKey);

        protected string? _IconKey;
        public virtual string? IconKey
        {
            get => _IconKey;
            protected set
            {
                this.RaiseAndSetIfChanged(ref _IconKey, value);
                this.RaisePropertyChanged(nameof(IconSource));
            }
        }

        #endregion

        #region Badge 変更通知

        private int _Badge;

        public virtual int Badge
        {
            get => _Badge;
            protected set => this.RaiseAndSetIfChanged(ref _Badge, value);
        }

        #endregion

        #region IsExpanded 変更通知

        private bool _IsExpanded;

        public virtual bool IsExpanded
        {
            get => _IsExpanded;
            protected set => this.RaiseAndSetIfChanged(ref _IsExpanded, value);
        }

        #endregion

        #region SelectsOnInvoked 変更通知

        private bool _SelectsOnInvoked = true;

        public virtual bool SelectsOnInvoked
        {
            get => _SelectsOnInvoked;
            protected set => this.RaiseAndSetIfChanged(ref _SelectsOnInvoked, value);
        }

        #endregion
    }
}
