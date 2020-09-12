using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels.Control
{
    public class StatusBarViewModel: Livet.ViewModel
    {
		#region NotificationMessage 変更通知

		private string _NotificationMessage;

		public string NotificationMessage
		{
			get { return this._NotificationMessage; }
			set
			{
				if (this._NotificationMessage != value)
				{
					this._NotificationMessage = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion
	}
}
