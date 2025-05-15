using MyApp.BusinessLogic;
using MyApp.DataLayer.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static MyApp.WPF.ComponentListViewModel;
using Component = MyApp.DataLayer.Models.Component;

namespace MyApp.WPF
{
    public class AssemblyBuilderViewModel : INotifyPropertyChanged
    {
        private readonly ComponentService _componentService;
        private readonly AssemblyService _assemblyService;

        public ObservableCollection<Component> AvailableComponents { get; set; }
        public ObservableCollection<Component> SelectedComponents { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public ICommand AddComponentCommand { get; }
        public ICommand RemoveComponentCommand { get; }
        public ICommand SaveAssemblyCommand { get; }

        private Component _selectedAvailable;
        public Component SelectedAvailable
        {
            get => _selectedAvailable;
            set { _selectedAvailable = value; OnPropertyChanged(nameof(SelectedAvailable)); }
        }

        private Component _selectedInBuild;
        public Component SelectedInBuild
        {
            get => _selectedInBuild;
            set { _selectedInBuild = value; OnPropertyChanged(nameof(SelectedInBuild)); }
        }

        public AssemblyBuilderViewModel()
        {
            _componentService = new ComponentService();
            _assemblyService = new AssemblyService();

            AvailableComponents = new ObservableCollection<Component>(_componentService.GetAllComponents());
            SelectedComponents = new ObservableCollection<Component>();

            AddComponentCommand = new RelayCommand(AddComponent);
            RemoveComponentCommand = new RelayCommand(RemoveComponent);
            SaveAssemblyCommand = new RelayCommand(SaveAssembly);
        }

        private void AddComponent()
        {
            if (SelectedAvailable != null && !SelectedComponents.Contains(SelectedAvailable))
                SelectedComponents.Add(SelectedAvailable);
        }

        private void RemoveComponent()
        {
            if (SelectedInBuild != null)
                SelectedComponents.Remove(SelectedInBuild);
        }

        private void SaveAssembly()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Введите название сборки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                MessageBox.Show("Введите описание сборки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedComponents.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один компонент.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_assemblyService.CreateAssembly(Name, Description, SelectedComponents.Select(c => c.ComponentID).ToList(), App.NameId, out string errorMessage))
            {
                MessageBox.Show("Сборка сохранена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Name = "";
                Description = "";
                SelectedComponents.Clear();
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
            }
            else
            {
                MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}