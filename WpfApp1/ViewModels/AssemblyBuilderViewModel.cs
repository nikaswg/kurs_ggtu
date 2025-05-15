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
            set 
            { 
                _selectedAvailable = value; 
                OnPropertyChanged(nameof(SelectedAvailable));
                ((RelayCommand)AddComponentCommand).RaiseCanExecuteChanged();
            }
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

            AddComponentCommand = new RelayCommand(AddComponent, CanAddComponent);
            RemoveComponentCommand = new RelayCommand(RemoveComponent);
            SaveAssemblyCommand = new RelayCommand(SaveAssembly);
        }

        private bool CanAddComponent()
        {
            if (SelectedAvailable == null) return false;

            var componentType = SelectedAvailable.Category?.Name;
            var selectedCountOfType = SelectedComponents.Count(c => c.Category?.Name == componentType);

            // Определяем максимальное количество для каждого типа
            int maxCount = componentType switch
            {
                "Процессор" => 1,
                "Материнская плата" => 1,
                "Блок питания" => 1,
                "Корпус" => 1,
                "Видеокарта" => 2,
                "Оперативная память" => 2,
                "SSD" or "HDD" => 3,
                "Охлаждение" => 1,
                _ => int.MaxValue // Для других типов без ограничений
            };

            return selectedCountOfType < maxCount;
        }

        private void AddComponent()
        {
            if (SelectedAvailable != null)
            {
                // Просто добавляем компонент, даже если такой уже есть
                SelectedComponents.Add(SelectedAvailable);
                (AddComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }


        private void RemoveComponent()
        {
            if (SelectedInBuild != null)
            {
                SelectedComponents.Remove(SelectedInBuild);
                ((RelayCommand)AddComponentCommand).RaiseCanExecuteChanged();
            }
        }

        private void SaveAssembly()
        {
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

            // Проверка обязательных компонентов
            var requiredComponents = new[] { "Процессор", "Материнская плата", "Блок питания", "Корпус" };
            var missingComponents = requiredComponents
                .Where(rc => !SelectedComponents.Any(c => c.Category?.Name == rc))
                .ToList();

            if (missingComponents.Any())
            {
                MessageBox.Show($"Отсутствуют обязательные компоненты: {string.Join(", ", missingComponents)}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_assemblyService.CreateAssembly(Description, SelectedComponents.Select(c => c.ComponentID).ToList(), App.NameId, out string errorMessage))
            {
                MessageBox.Show("Сборка сохранена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Description = "";
                SelectedComponents.Clear();
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