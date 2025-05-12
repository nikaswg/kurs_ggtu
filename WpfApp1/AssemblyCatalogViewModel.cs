using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MyApp.BusinessLogic;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using static MyApp.WPF.ComponentListViewModel;

namespace MyApp.WPF
{
    public class AssemblyCatalogViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _dbContext;
        private bool _isSortAscending = true;
        private string _currentUserEmail;

        public ObservableCollection<Assembly> Assemblies { get; set; }
        public bool IsAdmin => App.Role == "Admin";
        public string CurrentUserEmail => string.IsNullOrEmpty(App.Email) ? "Guest" : App.Email;

        public ICommand LikeAssemblyCommand { get; }
        public ICommand DislikeAssemblyCommand { get; }
        public ICommand DeleteAssemblyCommand { get; }
        public ICommand ShowReviewsCommand { get; }
        public ICommand SortByRatingCommand { get; }
        public ICommand SortByRatingDescCommand { get; }
        public ICommand CloseCommand { get; }

        public AssemblyCatalogViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            Assemblies = new ObservableCollection<Assembly>();

            // Initialize commands
            LikeAssemblyCommand = new RelayCommand<int>(LikeAssembly);
            DislikeAssemblyCommand = new RelayCommand<int>(DislikeAssembly);
            DeleteAssemblyCommand = new RelayCommand<int>(DeleteAssembly, _ => IsAdmin);
            ShowReviewsCommand = new RelayCommand<int>(ShowReviews);
            SortByRatingCommand = new RelayCommand(SortByRating);
            SortByRatingDescCommand = new RelayCommand(SortByRatingDesc);
            CloseCommand = new RelayCommand(CloseWindow);

            LoadAssembliesAsync();
        }

        private async void LoadAssembliesAsync()
        {
            var assemblies = await _dbContext.Assemblies
                .Include(a => a.User)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Reviews)
                    .ThenInclude(r => r.User)
                .AsNoTracking()
                .ToListAsync();

            Assemblies.Clear();
            foreach (var assembly in assemblies)
            {
                Assemblies.Add(assembly);
            }
        }

        public void LikeAssembly(int assemblyId)
        {
            var assembly = _dbContext.Assemblies.Find(assemblyId);
            if (assembly != null)
            {
                assembly.Rating++;
                _dbContext.SaveChanges();
                RefreshAssembly(assemblyId);
            }
        }

        public void DislikeAssembly(int assemblyId)
        {
            var assembly = _dbContext.Assemblies.Find(assemblyId);
            if (assembly != null)
            {
                assembly.Rating--;
                _dbContext.SaveChanges();
                RefreshAssembly(assemblyId);
            }
        }

        private void DeleteAssembly(int assemblyId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту сборку?", "Подтверждение удаления",
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var assembly = _dbContext.Assemblies
                    .Include(a => a.AssemblyComponents)
                    .Include(a => a.Reviews)
                    .FirstOrDefault(a => a.AssemblyID == assemblyId);

                if (assembly != null)
                {
                    _dbContext.AssemblyComponents.RemoveRange(assembly.AssemblyComponents);
                    _dbContext.Reviews.RemoveRange(assembly.Reviews);
                    _dbContext.Assemblies.Remove(assembly);
                    _dbContext.SaveChanges();

                    var assemblyToRemove = Assemblies.FirstOrDefault(a => a.AssemblyID == assemblyId);
                    if (assemblyToRemove != null)
                    {
                        Assemblies.Remove(assemblyToRemove);
                    }
                }
            }
        }

        private void ShowReviews(int assemblyId)
        {
            var assembly = _dbContext.Assemblies
                .Include(a => a.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefault(a => a.AssemblyID == assemblyId);

            if (assembly != null)
            {
                var reviewWindow = new ReviewsWindow(assembly, CurrentUserEmail, _dbContext);
                reviewWindow.ShowDialog();
                LoadAssembliesAsync(); // Refresh after possible new review
            }
        }

        private void SortByRating()
        {
            var sorted = Assemblies.OrderBy(a => a.Rating).ToList();
            Assemblies.Clear();
            foreach (var assembly in sorted)
            {
                Assemblies.Add(assembly);
            }
        }

        private void SortByRatingDesc()
        {
            var sorted = Assemblies.OrderByDescending(a => a.Rating).ToList();
            Assemblies.Clear();
            foreach (var assembly in sorted)
            {
                Assemblies.Add(assembly);
            }
        }

        private void RefreshAssembly(int assemblyId)
        {
            var updatedAssembly = _dbContext.Assemblies
                .AsNoTracking()
                .FirstOrDefault(a => a.AssemblyID == assemblyId);

            var oldAssembly = Assemblies.FirstOrDefault(a => a.AssemblyID == assemblyId);
            if (oldAssembly != null && updatedAssembly != null)
            {
                var index = Assemblies.IndexOf(oldAssembly);
                Assemblies[index] = updatedAssembly;
            }
        }

        private void CloseWindow()
        {
            Application.Current.Windows.OfType<AssemblyCatalogWindow>().FirstOrDefault()?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}