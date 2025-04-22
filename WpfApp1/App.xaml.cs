using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp1;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string Name { get; set; } = "-";
    public static string Email { get; set; } = "-";
    public static string Role { get; set; } = "Guest";
}

