using ReactiveUI;
using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public abstract partial class TabItemViewModel<TabItemId> : ItemViewModel where TabItemId : struct, Enum
    {
        public virtual bool IsTaskBarSubMenu { get; }

        IList<MenuItemViewModel>? _MenuItems;
        public virtual IList<MenuItemViewModel>? MenuItems
        {
            get => _MenuItems;
            protected set => this.RaiseAndSetIfChanged(ref _MenuItems, value);
        }

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

        private int _Badge = 0;

        public virtual int Badge
        {
            get => _Badge;
            protected set => this.RaiseAndSetIfChanged(ref _Badge, value);
        }

        #endregion

        #region Status 変更通知

        //private ViewModelBase? _Status;

        ///// <summary>
        ///// 获取状态栏中显示的状态
        ///// </summary>
        //public virtual ViewModelBase? Status
        //{
        //    get => _Status;
        //    protected set => this.RaiseAndSetIfChanged(ref _Status, value);
        //}

        #endregion

        protected TabItemViewModel()
        {
            if (IsInDesignMode) return;

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(Name));
            }).AddTo(this);
        }

        public virtual void Initialize()
        {
        }

        public virtual TabItemId Id { get; }

        public abstract string Name { get; protected set; }
    }
}
