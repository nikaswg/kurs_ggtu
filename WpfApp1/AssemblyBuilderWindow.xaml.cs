using System.Windows;

namespace MyApp.WPF
{
    public partial class AssemblyBuilderWindow : Window
    {
        public AssemblyBuilderWindow()
        {
            InitializeComponent();
            DataContext = new AssemblyBuilderViewModel();
        }
    }
}