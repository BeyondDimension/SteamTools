using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTools.Models;
using Livet;
using MetroRadiance.UI.Controls;
using Livet.EventListeners;
using SteamTools.Services;

namespace SteamTools.ViewModels
{
	public abstract class TabItemViewModel : ItemViewModel, ITabItem
	{
		#region Name 変更通知

		public abstract string Name { get; protected set; }

		#endregion

		#region Badge 変更通知

		private int? _Badge;

		public virtual int? Badge
		{
			get { return this._Badge; }
			protected set
			{
				if (this._Badge != value)
				{
					this._Badge = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Status 変更通知

		private ViewModel _Status;

		/// <summary>
		/// 获取状态栏中显示的状态
		/// </summary>
		public virtual ViewModel Status
		{
			get { return this._Status; }
			protected set
			{
				if (this._Status != value)
				{
					this._Status = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		protected TabItemViewModel()
		{
			if (Models.Helper.IsInDesignMode) return;

			this.CompositeDisposable.Add(new PropertyChangedEventListener(ResourceService.Current)
			{
				(sender, args) => this.RaisePropertyChanged(nameof(this.Name)),
			});
		}
	}
}
