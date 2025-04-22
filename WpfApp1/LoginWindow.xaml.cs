using System;
using System.Windows;
using MyApp.DataLayer.Services;
using WpfApp1;

namespace MyApp
{
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService;

        public LoginWindow()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = _userService.Authenticate(EmailTextBox.Text, PasswordBox.Password);
                if (user != null)
                {
                    App.Name = user.Name;
                    App.Email = user.Email;
                    App.Role = user.Role;
                    MessageBox.Show("Вход выполнен успешно.");
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на окно регистрации
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}