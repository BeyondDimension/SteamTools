using MetroRadiance.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SteamTools.Views.Content
{
    /// <summary>
    /// Game.xaml 的交互逻辑
    /// </summary>
    public partial class GameCSGO : UserControl
    {
        public GameCSGO()
        {
            InitializeComponent();
        }

        private void PromptTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as PromptTextBox).ScrollToEnd();
        }
    }
}
