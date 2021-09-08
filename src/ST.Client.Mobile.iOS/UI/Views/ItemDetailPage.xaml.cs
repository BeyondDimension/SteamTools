using System.Application.UI.ViewModels;
using Xamarin.Forms;

namespace System.Application.UI.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}