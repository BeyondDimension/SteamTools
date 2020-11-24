using MetroTrilithon.Mvvm;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class DialogWindowViewModel : MainWindowViewModelBase
    {
        public DialogWindowViewModel() : base()
        {
            this.DialogResult = false;
        }

        private string _Content;

        public string Content
        {
            get { return _Content; }
            set
            {
                if (this._Content != value)
                {
                    this._Content = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void OK()
        {
            this.DialogResult = true;
            this.Close();
        }

        public void Cancel()
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
