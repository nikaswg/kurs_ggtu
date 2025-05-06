using System.ComponentModel;
using System.Windows.Input;
using MyApp.BusinessLogic;
using MyApp.DataLayer.Models;
using static MyApp.WPF.ComponentListViewModel;
using Component = MyApp.DataLayer.Models.Component; // Добавьте это

namespace MyApp.WPF
{
    public class AddComponentViewModel : INotifyPropertyChanged
    {
        private readonly ComponentService _componentService;
        private Component _newComponent = new Component();

        public AddComponentViewModel()
        {
            _componentService = new ComponentService();
            AddCommand = new RelayCommand(AddComponent);
        }

        public Component NewComponent
        {
            get => _newComponent;
            set
            {
                _newComponent = value;
                OnPropertyChanged(nameof(NewComponent));
            }
        }

        public ICommand AddCommand { get; }

        private void AddComponent()
        {
            _componentService.AddComponent(NewComponent);
            NewComponent = new Component(); // Сброс формы после добавления
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}