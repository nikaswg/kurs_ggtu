using MyApp.BusinessLogic;
using MyApp.DataLayer;
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
        public ObservableCollection<Component> AvailableComponents { get; }
        public ObservableCollection<Component> SelectedComponents { get; }

        private Component _selectedAvailable;
        public Component SelectedAvailable
        {
            get => _selectedAvailable;
            set
            {
                _selectedAvailable = value;
                OnPropertyChanged(nameof(SelectedAvailable));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private Component _selectedInBuild;
        public Component SelectedInBuild
        {
            get => _selectedInBuild;
            set
            {
                _selectedInBuild = value;
                OnPropertyChanged(nameof(SelectedInBuild));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddComponentCommand { get; }
        public ICommand RemoveComponentCommand { get; }
        public ICommand SaveAssemblyCommand { get; }

        public AssemblyBuilderViewModel()
        {
            var service = new ComponentService();
            AvailableComponents = new ObservableCollection<Component>(service.GetAllComponents());
            SelectedComponents = new ObservableCollection<Component>();

            AddComponentCommand = new ORelayCommand(
                () =>
                {
                    if (SelectedAvailable != null)
                    {
                        var category = SelectedAvailable.Category?.Name;
                        var currentCount = SelectedComponents.Count(c => c.Category?.Name == category);

                        if (CanAddComponent(category, currentCount))
                        {
                            SelectedComponents.Add(SelectedAvailable);
                            AvailableComponents.Remove(SelectedAvailable);
                        }
                        else
                        {
                            ShowLimitMessage(category);
                        }
                    }
                },
                () => SelectedAvailable != null && CanAddComponent(SelectedAvailable.Category?.Name,
                      SelectedComponents.Count(c => c.Category?.Name == SelectedAvailable.Category?.Name))
            );

            RemoveComponentCommand = new ORelayCommand(
                () =>
                {
                    if (SelectedInBuild != null)
                    {
                        AvailableComponents.Add(SelectedInBuild);
                        SelectedComponents.Remove(SelectedInBuild);
                    }
                },
                () => SelectedInBuild != null
            );

            SaveAssemblyCommand = new ORelayCommand(
                () =>
                {
                    if (string.IsNullOrWhiteSpace(Description))
                    {
                        MessageBox.Show("Введите описание сборки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!SelectedComponents.Any())
                    {
                        MessageBox.Show("Добавьте хотя бы один компонент", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!ValidateRequiredComponents())
                    {
                        return;
                    }

                    try
                    {
                        using (var context = new AppDbContext())
                        {
                            // Создаем новую сборку
                            var assembly = new Assembly
                            {
                                Description = Description,
                                NameId = App.NameId, // Предполагается, что у вас есть текущий пользователь
                                Rating = 0,
                                User = context.Users.FirstOrDefault(u => u.NameId == App.NameId)
                            };

                            // Добавляем сборку в контекст
                            context.Assemblies.InsertOnSubmit(assembly);
                            context.SubmitChanges(); // Сохраняем, чтобы получить AssemblyID

                            // Добавляем компоненты в сборку
                            foreach (var component in SelectedComponents)
                            {
                                var assemblyComponent = new AssemblyComponent
                                {
                                    AssemblyID = assembly.AssemblyID,
                                    ComponentID = component.ComponentID
                                };
                                context.AssemblyComponents.InsertOnSubmit(assemblyComponent);
                            }

                            context.SubmitChanges(); // Сохраняем связи с компонентами

                            MessageBox.Show("Сборка сохранена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Очищаем форму после сохранения
                            SelectedComponents.Clear();
                            Description = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении сборки: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                () => !string.IsNullOrWhiteSpace(Description) && SelectedComponents.Any()
            );
        }

        private bool CanAddComponent(string category, int currentCount)
        {
            return category switch
            {
                "Процессор" => currentCount < 1,
                "Материнская плата" => currentCount < 1,
                "Блок питания" => currentCount < 1,
                "Корпус" => currentCount < 1,
                "Видеокарта" => currentCount < 2,
                "Оперативная память" => currentCount < 2,
                "SSD" or "HDD" => currentCount < 2,
                _ => true // Для других типов без ограничений
            };
        }

        private void ShowLimitMessage(string category)
        {
            string message = category switch
            {
                "Процессор" => "Можно добавить только 1 процессор",
                "Материнская плата" => "Можно добавить только 1 материнскую плату",
                "Блок питания" => "Можно добавить только 1 блок питания",
                "Корпус" => "Можно добавить только 1 корпус",
                "Видеокарта" => "Можно добавить максимум 2 видеокарты",
                "Оперативная память" => "Можно добавить максимум 2 модуля памяти",
                "SSD" or "HDD" => "Можно добавить максимум 2 накопителя",
                _ => "Достигнуто максимальное количество для этого типа компонента"
            };

            MessageBox.Show(message, "Ограничение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool ValidateRequiredComponents()
        {
            var required = new[] { "Процессор", "Материнская плата", "Блок питания", "Корпус" };
            var missing = required.Except(
                SelectedComponents.Select(c => c.Category?.Name).Distinct()).ToList();

            if (missing.Any())
            {
                MessageBox.Show($"Отсутствуют обязательные компоненты:\n{string.Join("\n", missing)}",
                    "Неполная сборка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ORelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public ORelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }
}