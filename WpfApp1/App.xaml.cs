using System.Windows;
using MyApp.BusinessLogic;
using MyApp.WPF; // Убедитесь, что это правильное пространство имен

using MyApp.BusinessLogicLayer.Services;

namespace MyApp
{
    public partial class App : Application
    {

        public static int NameId { get; set; } = 0;
        public static string Name { get; set; } = "-";
        public static string Email { get; set; } = "-";
        public static string Role { get; set; } = "Guest";
    }
}