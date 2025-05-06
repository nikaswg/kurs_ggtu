using System.Windows;
using MyApp.BusinessLogic;
using MyApp.WPF;

namespace WpfApp1
{
    public partial class AddComponentWindow : Window
    {
        public AddComponentWindow(AddComponentViewModel viewModel)
        {
            InitializeComponent(); // Должен быть первым!
            DataContext = viewModel;
        }
    }
}