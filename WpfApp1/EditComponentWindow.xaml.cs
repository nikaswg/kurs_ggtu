using System.Windows;

namespace MyApp.WPF
{
    public partial class EditComponentWindow : Window
    {
        public EditComponentWindow(EditComponentViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}