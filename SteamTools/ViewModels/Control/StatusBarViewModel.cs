using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels.Control
{
    public class StatusBarViewModel: Livet.ViewModel
    {
		#region StatusText 変更通知

		private string _StatusText;

		public string StatusText
		{
			get { return this._StatusText; }
			set
			{
				if (this._StatusText != value)
				{
					this._StatusText = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Badge 変更通知

		private string _Badge;

		public string Badge
		{
			get { return this._Badge; }
			set
			{
				if (this._Badge != value)
				{
					this._Badge = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

	}
}
