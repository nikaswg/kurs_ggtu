using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MyApp.BusinessLogic;
using System.Linq;
using Component = MyApp.DataLayer.Models.Component; // Добавьте это
using WpfApp1;

namespace MyApp.WPF
{
    public class ComponentListViewModel : INotifyPropertyChanged
    {
        private readonly ComponentService _componentService;
        private ObservableCollection<Component> _components;
        private string _searchText;
        private decimal? _minPrice;
        private decimal? _maxPrice;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;

        public ComponentListViewModel()
        {
            _componentService = new ComponentService();
            LoadComponents();
            SearchCommand = new RelayCommand(Search);
            FilterCommand = new RelayCommand(Filter);
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            ShowAddWindowCommand = new RelayCommand(ShowAddWindow);
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

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
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

        public ICommand SearchCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public ICommand ShowAddWindowCommand { get; }

        public bool HasPreviousPage => _currentPage > 1;
        public bool HasNextPage => _currentPage < TotalPages;
        public int TotalPages => (int)Math.Ceiling((double)_totalCount / _pageSize);
        public string PageInfo => $"Страница {_currentPage} из {TotalPages}";

        private void LoadComponents()
        {
            var pagedResult = _componentService.GetComponentsPaged(_currentPage, _pageSize);
            Components = new ObservableCollection<Component>(pagedResult.Results);
            _totalCount = pagedResult.TotalCount;
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        }

        private void Search()
        {
            var results = _componentService.SearchComponents(SearchText);
            Components = new ObservableCollection<Component>(results);
        }

        private void Filter()
        {
            var results = _componentService.FilterComponents(MinPrice, MaxPrice);
            Components = new ObservableCollection<Component>(results);
        }

        private void NextPage()
        {
            if (CanGoNextPage())
            {
                _currentPage++;
                LoadComponents();
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(PageInfo));
            }
        }

        private bool CanGoNextPage()
        {
            return _currentPage < TotalPages;
        }

        private void PreviousPage()
        {
            if (CanGoPreviousPage())
            {
                _currentPage--;
                LoadComponents();
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(PageInfo));
            }
        }

        private bool CanGoPreviousPage()
        {
            return _currentPage > 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Простая реализация ICommand
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

        private void ShowAddWindow()
        {
            var viewModel = new AddComponentViewModel();
            var addWindow = new AddComponentWindow(viewModel);
            addWindow.ShowDialog();
            LoadComponents(); // Обновляем список после добавления
        }
    }
}