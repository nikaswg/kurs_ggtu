using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MyApp.BusinessLogic;
using WpfApp1;
using DataComponent = MyApp.DataLayer.Models.Component; // Алиас для устранения неоднозначности

namespace MyApp.WPF
{
    public class ComponentListViewModel : INotifyPropertyChanged
    {
        private readonly ComponentService _componentService;
        private ObservableCollection<DataComponent> _components;
        private string _searchText;
        private string _nameSearchText;
        private decimal? _minPrice;
        private decimal? _maxPrice;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private DataComponent _selectedComponent;
        private bool _exactMatch;
        private Dictionary<string, int> _sortStates = new Dictionary<string, int>
        {
            {"Name", 0},
            {"Description", 0},
            {"Price", 0},
            {"Category.Name", 0}
        };

        public ComponentListViewModel()
        {
            _componentService = new ComponentService();
            LoadComponents();

            // Инициализация команд
            SearchCommand = new RelayCommand(Search);
            NameSearchCommand = new RelayCommand(NameSearch);
            ResetNameSearchCommand = new RelayCommand(ResetNameSearch);
            FilterCommand = new RelayCommand(Filter);
            ResetFilterCommand = new RelayCommand(ResetFilter);
            NextPageCommand = new RelayCommand(NextPage, CanGoNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, CanGoPreviousPage);
            ShowAddWindowCommand = new RelayCommand(ShowAddWindow, () => App.Role == "Admin");
            EditComponentCommand = new RelayCommand<DataComponent>(EditComponent, c => App.Role == "Admin");
            SortCommand = new RelayCommand<string>(Sort);
        }

        // Свойства
        public ObservableCollection<DataComponent> Components
        {
            get => _components;
            set
            {
                _components = value;
                OnPropertyChanged(nameof(Components));
            }
        }

        public DataComponent SelectedComponent
        {
            get => _selectedComponent;
            set
            {
                _selectedComponent = value;
                OnPropertyChanged(nameof(SelectedComponent));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public string NameSearchText
        {
            get => _nameSearchText;
            set
            {
                _nameSearchText = value;
                OnPropertyChanged(nameof(NameSearchText));
            }
        }

        public decimal? MinPrice
        {
            get => _minPrice;
            set
            {
                _minPrice = value;
                OnPropertyChanged(nameof(MinPrice));
            }
        }

        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                _maxPrice = value;
                OnPropertyChanged(nameof(MaxPrice));
            }
        }

        public bool ExactMatch
        {
            get => _exactMatch;
            set
            {
                _exactMatch = value;
                OnPropertyChanged(nameof(ExactMatch));
            }
        }

        public bool HasPreviousPage => _currentPage > 1;
        public bool HasNextPage => _currentPage < TotalPages;
        public int TotalPages => (int)Math.Ceiling((double)_totalCount / _pageSize);
        public string PageInfo => $"Страница {_currentPage} из {TotalPages}";
        public bool IsAdmin => App.Role == "Admin";

        // Команды
        public ICommand SearchCommand { get; }
        public ICommand NameSearchCommand { get; }
        public ICommand ResetNameSearchCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ShowAddWindowCommand { get; }
        public ICommand EditComponentCommand { get; }
        public ICommand SortCommand { get; }

        // Методы
        private void LoadComponents()
        {
            var pagedResult = _componentService.GetComponentsPaged(_currentPage, _pageSize);
            Components = new ObservableCollection<DataComponent>(pagedResult.Results);
            _totalCount = pagedResult.TotalCount;
            UpdatePaginationProperties();
        }

        private void Search()
        {
            try
            {
                var results = _componentService.SearchComponents(
                    searchText: SearchText,
                    minPrice: MinPrice,
                    maxPrice: MaxPrice,
                    exactMatch: ExactMatch);

                Components = new ObservableCollection<DataComponent>(results);
                ResetSortStates();

                if (!results.Any())
                {
                    MessageBox.Show("Ничего не найдено", "Результаты поиска",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NameSearch()
        {
            try
            {
                var results = _componentService.SearchByName(
                    name: NameSearchText,
                    exactMatch: ExactMatch);

                Components = new ObservableCollection<DataComponent>(results);
                ResetSortStates();

                if (!results.Any())
                {
                    MessageBox.Show("Ничего не найдено", "Результаты поиска",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetNameSearch()
        {
            NameSearchText = string.Empty;
            LoadComponents();
        }

        private void Filter()
        {
            var results = _componentService.SearchComponents(
                minPrice: MinPrice,
                maxPrice: MaxPrice);

            Components = new ObservableCollection<DataComponent>(results);
            ResetSortStates();
        }

        private void ResetFilter()
        {
            MinPrice = null;
            MaxPrice = null;
            LoadComponents();
        }

        private void Sort(string field)
        {
            var direction = _sortStates[field];
            var componentsList = Components.ToList();
            var sortedList = _componentService.SortComponents(
                components: componentsList,
                sortField: field,
                sortDirection: ref direction);

            _sortStates[field] = direction;
            Components = new ObservableCollection<DataComponent>(sortedList);
        }

        private void ResetSortStates()
        {
            foreach (var key in _sortStates.Keys.ToList())
            {
                _sortStates[key] = 0;
            }
        }

        private void NextPage()
        {
            _currentPage++;
            LoadComponents();
        }

        private bool CanGoNextPage() => _currentPage < TotalPages;

        private void PreviousPage()
        {
            _currentPage--;
            LoadComponents();
        }

        private bool CanGoPreviousPage() => _currentPage > 1;

        private void ShowAddWindow()
        {
            var viewModel = new AddComponentViewModel();
            var addWindow = new AddComponentWindow(viewModel);
            addWindow.ShowDialog();
            LoadComponents();
        }

        private void EditComponent(DataComponent component)
        {
            if (component == null) return;

            var editViewModel = new EditComponentViewModel(component);
            var editWindow = new EditComponentWindow(editViewModel);

            if (editWindow.ShowDialog() == true)
            {
                _componentService.UpdateComponent(component);
                LoadComponents();
            }
        }

        private void UpdatePaginationProperties()
        {
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Классы команд
        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
            public void Execute(object parameter) => _execute();
        }

        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            private readonly Func<T, bool> _canExecute;

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
            public void Execute(object parameter) => _execute((T)parameter);
        }
    }
}