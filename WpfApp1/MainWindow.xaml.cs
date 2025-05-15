using MyApp.DataLayer;
using MyApp.WPF;
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
            IdTextBlock.Text = App.NameId.ToString();
            AuthButton.Content = (App.Role == "Guest") ? "Войти" : "Выйти";
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
            if (App.Role == "Guest")
            {
                MessageBox.Show("Только зарегистрированные пользователи могут просматривать комплектующие", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                var viewModel = new ComponentListViewModel(); // Создаем экземпляр ViewModel
                var componentList = new ComponentListPage(viewModel); // Передаем его в конструктор
                componentList.ShowDialog();
            }
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.Role == "Guest")
            {
                MessageBox.Show("Только зарегистрированные пользователи могут просматривать комплектующие", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                var builderWindow = new AssemblyBuilderWindow();
                builderWindow.ShowDialog();
            }
        }

        private void ACatalogButton_Click(object sender, RoutedEventArgs e)
        {
            {
                using (var dbContext = new AppDbContext())
                {
                    var catalogWindow = new AssemblyCatalogWindow(dbContext);
                    catalogWindow.ShowDialog();
                }
            }
        }
    }
}