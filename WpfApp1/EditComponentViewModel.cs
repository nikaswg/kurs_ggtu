using MyApp.DataLayer.Models;
using System.Windows;

namespace MyApp.WPF
{
    public class EditComponentViewModel
    {
        public Component Component { get; set; }

        public EditComponentViewModel(Component component)
        {
            Component = component;
        }
    }
}