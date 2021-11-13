using SIO_AgendaXamarin.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace SIO_AgendaXamarin.Views
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