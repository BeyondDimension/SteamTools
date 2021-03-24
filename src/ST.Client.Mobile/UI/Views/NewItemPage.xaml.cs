using System.Application.Models;
using System.Application.UI.ViewModels;
using Xamarin.Forms;

namespace System.Application.UI.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}