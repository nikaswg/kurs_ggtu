﻿using System;
using System.Windows;
using MyApp.BusinessLogicLayer.Services;
using MyApp.DataLayer;

namespace MyApp
{
    public partial class RegisterWindow : Window
    {
        private readonly UserService _userService;

        public RegisterWindow()
        {
            InitializeComponent();
            var dbContext = new AppDbContext();
            _userService = new UserService(dbContext);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userService.Register(NameTextBox.Text, EmailTextBox.Text, PasswordBox.Password);
                MessageBox.Show("Регистрация успешна.");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}