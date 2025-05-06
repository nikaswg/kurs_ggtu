using System.Windows;
using WpfApp1;

namespace MyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateUserInfo();
        }

        private void UpdateUserInfo()
        {
            NameTextBlock.Text = App.Name;
            EmailTextBlock.Text = App.Email;
            RoleTextBlock.Text = App.Role;
            AuthButton.Content = (App.Role == "Guest" ) ? "Войти" : "Выйти";
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.Role == "Guest") 
            {
                var loginWindow = new LoginWindow();
                loginWindow.ShowDialog();
            }
            else
            {
                App.Name = "-";
                App.Email = "-";
                App.Role = "Guest";
            }
            UpdateUserInfo();
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Переход в каталог. (Реализуйте переход на другую страницу)");
        }
    }
}