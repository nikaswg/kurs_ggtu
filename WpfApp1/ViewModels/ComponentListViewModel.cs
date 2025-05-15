using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MyApp.BusinessLogic;
using System.Linq;
using Component = MyApp.DataLayer.Models.Component;
using WpfApp1;
using System.Windows;

namespace MyApp.WPF
{
    public class ComponentListViewModel : INotifyPropertyChanged
    {
        private readonly ComponentService _componentService;
        private ObservableCollection<Component> _components;
        private string _searchText;
        private string _nameSearchText;
        private decimal? _minPrice;
        private decimal? _maxPrice;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private Component _selectedComponent;
        private Dictionary<string, int> _sortStates = new Dictionary<string, int>
        {
            {"Name", 0},
            {"Description", 0},
            {"Price", 0},
            {"Category.Name", 0}
        };
        private bool _exactMatch;

        public ComponentListViewModel()
        {
            _componentService = new ComponentService();
            LoadComponents();
            SearchCommand = new RelayCommand(Search);
            FilterCommand = new RelayCommand(Filter);
            NameSearchCommand = new RelayCommand(NameSearch);
            ResetNameSearchCommand = new RelayCommand(ResetNameSearch);
            ResetFilterCommand = new RelayCommand(ResetFilter);
            NextPageCommand = new RelayCommand(NextPage, CanGoNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, CanGoPreviousPage);
            ShowAddWindowCommand = new RelayCommand(ShowAddWindow, () => App.Role == "Admin");
            EditComponentCommand = new RelayCommand<Component>(EditComponent, c => App.Role == "Admin");
            SortCommand = new RelayCommand<string>(Sort);
        }

        public ObservableCollection<Component> Components
        {
            get { return _components; }
            set
            {
                _components = value;
                OnPropertyChanged(nameof(Components));
            }
        }

        public Component SelectedComponent
        {
            get { return _selectedComponent; }
            set
            {
                _selectedComponent = value;
                OnPropertyChanged(nameof(SelectedComponent));
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public string NameSearchText
        {
            get { return _nameSearchText; }
            set
            {
                _nameSearchText = value;
                OnPropertyChanged(nameof(NameSearchText));
            }
        }

        public decimal? MinPrice
        {
            get { return _minPrice; }
            set
            {
                _minPrice = value;
                OnPropertyChanged(nameof(MinPrice));
            }
        }

        public decimal? MaxPrice
        {
            get { return _maxPrice; }
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

        public bool HasPreviousPage => _currentPage > 1;
        public bool HasNextPage => _currentPage < TotalPages;
        public int TotalPages => (int)Math.Ceiling((double)_totalCount / _pageSize);
        public string PageInfo => $"Страница {_currentPage} из {TotalPages}";
        public bool IsAdmin => App.Role == "Admin";

        private void LoadComponents()
        {
            var pagedResult = _componentService.GetComponentsPaged(_currentPage, _pageSize);
            Components = new ObservableCollection<Component>(pagedResult.Results);
            _totalCount = pagedResult.TotalCount;
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(IsAdmin));
        }

        private void Search()
        {
            try
            {
                var results = _componentService.SearchComponents(
                    SearchText,
                    MinPrice,
                    MaxPrice,
                    ExactMatch);

                Components = new ObservableCollection<Component>(results);
                
                if (!results.Any())
                {
                    MessageBox.Show("Ничего не найдено", "Результаты поиска",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ResetSortStates();
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
                var results = _componentService.SearchComponentsByName(
                    NameSearchText,
                    ExactMatch);

                Components = new ObservableCollection<Component>(results);
                
                if (!results.Any())
                {
                    MessageBox.Show("Ничего не найдено", "Результаты поиска",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ResetSortStates();
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
            var results = _componentService.FilterComponents(MinPrice, MaxPrice);
            Components = new ObservableCollection<Component>(results);
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
            var sortedList = _componentService.SortComponents(Components.ToList(), field, ref direction);
            _sortStates[field] = direction;

            Components = new ObservableCollection<Component>(sortedList);
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

        private bool CanGoNextPage()
        {
            return _currentPage < TotalPages;
        }

        private void PreviousPage()
        {
            _currentPage--;
            LoadComponents();
        }

        private bool CanGoPreviousPage()
        {
            return _currentPage > 1;
        }

        private void ShowAddWindow()
        {
            var viewModel = new AddComponentViewModel();
            var addWindow = new AddComponentWindow(viewModel);
            addWindow.ShowDialog();
            LoadComponents();
        }

        private void EditComponent(Component component)
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute();
            }

            public void Execute(object parameter)
            {
                _execute();
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }

        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            private readonly Func<T, bool> _canExecute;

            public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute((T)parameter);
            }

            public void Execute(object parameter)
            {
                _execute((T)parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }
    }
}