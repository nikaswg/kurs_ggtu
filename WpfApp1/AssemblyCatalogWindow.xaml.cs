using System.Windows;
using System.Windows.Controls;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;

namespace MyApp.WPF
{
    public partial class AssemblyCatalogWindow : Window
    {
        public AssemblyCatalogWindow(AppDbContext dbContext)
        {
            InitializeComponent();
            DataContext = new AssemblyCatalogViewModel(dbContext);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Assembly assembly)
            {
                var viewModel = (AssemblyCatalogViewModel)DataContext;
                viewModel.LikeAssembly(assembly.AssemblyID);
                MessageBox.Show("Рейтинг увеличен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Dislike_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Assembly assembly)
            {
                var viewModel = (AssemblyCatalogViewModel)DataContext;
                viewModel.DislikeAssembly(assembly.AssemblyID);
                MessageBox.Show("Рейтинг уменьшен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}