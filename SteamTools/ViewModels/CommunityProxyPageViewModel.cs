using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Proxy;

namespace SteamTools.ViewModels
{
	public class CommunityProxyPageViewModel : TabItemViewModel
	{
		public override string Name
		{
			get { return Properties.Resources.Steam302; }
			protected set { throw new NotImplementedException(); }
		}

		private bool _ProxyStatus;
		public bool ProxyStatus
		{
			get { return _ProxyStatus; }
		    set
			{
				if (this._ProxyStatus != value)
				{
					this._ProxyStatus = value;
					if (value)
					{
						HttpProxy.Current.StartProxy();
					}
					else 
					{
						HttpProxy.Current.StopProxy();
					}
					this.RaisePropertyChanged();
				}
			}
		}

		public CommunityProxyPageViewModel() 
		{
			//HttpProxy.Current.SetupCertificate();
		}

		public void StatrService_OnClick() 
		{
			HttpProxy.Current.StartProxy();
		}

		public void SetupCertificate_OnClick()
		{
			HttpProxy.Current.SetupCertificate();
		}

		public void DeleteCertificate_OnClick()
		{
			HttpProxy.Current.DeleteCertificate();
		}
	}
}
