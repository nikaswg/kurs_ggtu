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

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var assembly = button.Tag as Assembly;
                if (assembly != null)
                {
                    var viewModel = DataContext as AssemblyCatalogViewModel;
                    viewModel.LikeAssembly(assembly, App.Email);
                    MessageBox.Show("Вы оценили сборку!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оценке сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Dislike_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var assembly = button.Tag as Assembly;
                if (assembly != null)
                {
                    var viewModel = DataContext as AssemblyCatalogViewModel;
                    viewModel.DislikeAssembly(assembly, App.Email);
                    MessageBox.Show("Вы сняли оценку с сборки!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при снятии оценки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}