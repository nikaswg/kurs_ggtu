using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using static MyApp.WPF.ComponentListViewModel;

namespace MyApp.WPF
{
    public class AssemblyCatalogViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _dbContext;
        private string _currentUserEmail;

        public ObservableCollection<Assembly> Assemblies { get; set; }
        public bool IsAdmin => App.Role == "Admin";
        public string CurrentUserEmail => string.IsNullOrEmpty(App.Email) ? "Guest" : App.Email;

        public ICommand LikeAssemblyCommand { get; private set; }
        public ICommand DislikeAssemblyCommand { get; private set;  }
        public ICommand DeleteAssemblyCommand { get; private set; }
        public ICommand ShowReviewsCommand { get; private set; }
        public ICommand SortByRatingCommand { get; private set; }
        public ICommand SortByRatingDescCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public AssemblyCatalogViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _currentUserEmail = CurrentUserEmail;
            Assemblies = new ObservableCollection<Assembly>();

            InitializeCommands();
            LoadAssembliesAsync();
        }

        public void InitializeCommands()
        {
            LikeAssemblyCommand = new RelayCommand<int>(LikeAssembly);
            DislikeAssemblyCommand = new RelayCommand<int>(DislikeAssembly);
            DeleteAssemblyCommand = new RelayCommand<int>(DeleteAssembly, _ => IsAdmin);
            ShowReviewsCommand = new RelayCommand<int>(ShowReviews);
            SortByRatingCommand = new RelayCommand(SortByRating);
            SortByRatingDescCommand = new RelayCommand(SortByRatingDesc);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private async void LoadAssembliesAsync()
        {
            try
            {
                var assemblies = await _dbContext.Assemblies
                    .Include(a => a.User)
                    .Include(a => a.AssemblyComponents)
                        .ThenInclude(ac => ac.Component) // Убедитесь, что загружаются компоненты
                    .Include(a => a.Reviews)
                        .ThenInclude(r => r.User)
                    .Include(a => a.UserVotes)
                    .AsNoTracking()
                    .ToListAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Assemblies.Clear();
                    foreach (var assembly in assemblies)
                    {
                        Assemblies.Add(assembly);
                        // Уведомляем об изменении TotalPrice
                        OnPropertyChanged(nameof(assembly.TotalPrice));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сборок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUserVote(int assemblyId, out UserVote existingVote)
        {
            existingVote = null;

            if (_currentUserEmail == "Guest")
            {
                MessageBox.Show("Гости не могут голосовать", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            existingVote = _dbContext.UserVotes
                .FirstOrDefault(v => v.AssemblyID == assemblyId && v.UserEmail == _currentUserEmail);

            if (existingVote != null)
            {
                MessageBox.Show("Вы уже голосовали за эту сборку", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public void LikeAssembly(int assemblyId)
        {
            try
            {
                if (!CanUserVote(assemblyId, out _)) return;

                var assembly = _dbContext.Assemblies.Find(assemblyId);
                if (assembly != null)
                {
                    assembly.Rating++;
                    _dbContext.UserVotes.Add(new UserVote
                    {
                        AssemblyID = assemblyId,
                        UserEmail = _currentUserEmail,
                        VoteType = 1 // 1 для лайка
                    });

                    SaveAndRefresh(assemblyId);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public void DislikeAssembly(int assemblyId)
        {
            try
            {
                if (!CanUserVote(assemblyId, out _)) return;

                var assembly = _dbContext.Assemblies.Find(assemblyId);
                if (assembly != null)
                {
                    assembly.Rating--;
                    _dbContext.UserVotes.Add(new UserVote
                    {
                        AssemblyID = assemblyId,
                        UserEmail = _currentUserEmail,
                        VoteType = 0 // 0 для дизлайка
                    });

                    SaveAndRefresh(assemblyId);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SaveAndRefresh(int assemblyId)
        {
            _dbContext.SaveChanges();
            RefreshAssembly(assemblyId);
        }

        private void RefreshAssembly(int assemblyId)
        {
            var updatedAssembly = _dbContext.Assemblies
                .Include(a => a.User)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Reviews)
                .Include(a => a.UserVotes)
                .AsNoTracking()
                .FirstOrDefault(a => a.AssemblyID == assemblyId);

            if (updatedAssembly != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var oldAssembly = Assemblies.FirstOrDefault(a => a.AssemblyID == assemblyId);
                    if (oldAssembly != null)
                    {
                        var index = Assemblies.IndexOf(oldAssembly);
                        Assemblies.RemoveAt(index);
                        Assemblies.Insert(index, updatedAssembly);
                    }
                });
            }
        }

        private void DeleteAssembly(int assemblyId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту сборку?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var assembly = _dbContext.Assemblies
                    .Include(a => a.AssemblyComponents)
                    .Include(a => a.Reviews)
                    .Include(a => a.UserVotes)
                    .FirstOrDefault(a => a.AssemblyID == assemblyId);

                if (assembly != null)
                {
                    _dbContext.AssemblyComponents.RemoveRange(assembly.AssemblyComponents);
                    _dbContext.Reviews.RemoveRange(assembly.Reviews);
                    _dbContext.UserVotes.RemoveRange(assembly.UserVotes);
                    _dbContext.Assemblies.Remove(assembly);
                    _dbContext.SaveChanges();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var assemblyToRemove = Assemblies.FirstOrDefault(a => a.AssemblyID == assemblyId);
                        if (assemblyToRemove != null)
                        {
                            Assemblies.Remove(assemblyToRemove);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowReviews(int assemblyId)
        {
            try
            {
                var assembly = _dbContext.Assemblies
                    .Include(a => a.Reviews)
                    .ThenInclude(r => r.User)
                    .FirstOrDefault(a => a.AssemblyID == assemblyId);

                if (assembly != null)
                {
                    var reviewWindow = new ReviewsWindow(assembly, CurrentUserEmail, _dbContext);
                    reviewWindow.ShowDialog();
                    LoadAssembliesAsync();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SortByRating()
        {
            var sorted = Assemblies.OrderBy(a => a.Rating).ToList();
            UpdateCollection(sorted);
        }

        private void SortByRatingDesc()
        {
            var sorted = Assemblies.OrderByDescending(a => a.Rating).ToList();
            UpdateCollection(sorted);
        }

        private void UpdateCollection(List<Assembly> sortedAssemblies)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Assemblies.Clear();
                foreach (var assembly in sortedAssemblies)
                {
                    Assemblies.Add(assembly);
                }
            });
        }

        private void HandleError(Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
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