using System.Windows;

namespace MyApp.WPF
{
    public partial class ComponentListPage : Window
    {
        private readonly ComponentListViewModel _viewModel;

        public ComponentListPage(ComponentListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}