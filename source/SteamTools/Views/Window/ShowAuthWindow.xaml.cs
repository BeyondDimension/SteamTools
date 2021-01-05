using SteamTools.Models;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteamTools
{
    public partial class ShowAuthWindow
    {
        public ShowAuthWindow()
        {
            InitializeComponent();
            App.Current.MainWindow.Closed += (sender, args) => this.Close();
        }
    }
}
