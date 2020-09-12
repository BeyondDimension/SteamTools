using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
	public class ItemViewModel : Livet.ViewModel
	{
		#region IsSelected 変更通知

		private bool _IsSelected;

		public virtual bool IsSelected
		{
			get { return this._IsSelected; }
			set
			{
				if (this._IsSelected != value)
				{
					this._IsSelected = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion
	}
}
