using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using MyApp.DataLayer;
using MyApp.DataLayer.Models;

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
        public ICommand DislikeAssemblyCommand { get; private set; }
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
            LoadAssemblies();
        }

        private void InitializeCommands()
        {
            LikeAssemblyCommand = new RelayCommand<int>(LikeAssembly);
            DislikeAssemblyCommand = new RelayCommand<int>(DislikeAssembly);
            DeleteAssemblyCommand = new RelayCommand<int>(DeleteAssembly, _ => IsAdmin);
            ShowReviewsCommand = new RelayCommand<int>(ShowReviews);
            SortByRatingCommand = new RelayCommand(SortByRating);
            SortByRatingDescCommand = new RelayCommand(SortByRatingDesc);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void LoadAssemblies()
        {
            try
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Assembly>(a => a.User);
                loadOptions.LoadWith<Assembly>(a => a.AssemblyComponents);
                loadOptions.LoadWith<AssemblyComponent>(ac => ac.Component);
                loadOptions.LoadWith<Assembly>(a => a.Reviews);
                loadOptions.LoadWith<Review>(r => r.User);
                loadOptions.LoadWith<Assembly>(a => a.UserVotes);

                using (var context = new AppDbContext())
                {
                    context.LoadOptions = loadOptions;
                    var assemblies = context.Assemblies.ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Assemblies.Clear();
                        foreach (var assembly in assemblies)
                        {
                            assembly.OnPropertyChanged(nameof(Assembly.TotalPrice));
                            Assemblies.Add(assembly);
                        }
                    });
                }
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

            using (var context = new AppDbContext())
            {
                existingVote = context.UserVotes
                    .FirstOrDefault(v => v.AssemblyID == assemblyId && v.UserEmail == _currentUserEmail);
            }

            if (App.NameId == 0)
            {
                MessageBox.Show("Только зарегистрированные пользователи могут голосовать", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

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

                using (var context = new AppDbContext())
                {
                    var assembly = context.Assemblies
                        .FirstOrDefault(a => a.AssemblyID == assemblyId);

                    if (assembly != null)
                    {
                        assembly.Rating++;

                        context.UserVotes.InsertOnSubmit(new UserVote
                        {
                            AssemblyID = assemblyId,
                            UserEmail = _currentUserEmail,
                            VoteType = 1
                        });

                        context.SubmitChanges();
                        RefreshAssembly(assemblyId);
                    }
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

                using (var context = new AppDbContext())
                {
                    var assembly = context.Assemblies
                        .FirstOrDefault(a => a.AssemblyID == assemblyId);

                    if (assembly != null)
                    {
                        assembly.Rating--;

                        context.UserVotes.InsertOnSubmit(new UserVote
                        {
                            AssemblyID = assemblyId,
                            UserEmail = _currentUserEmail,
                            VoteType = 0
                        });

                        context.SubmitChanges();
                        RefreshAssembly(assemblyId);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void RefreshAssembly(int assemblyId)
        {
            try
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Assembly>(a => a.User);
                loadOptions.LoadWith<Assembly>(a => a.AssemblyComponents);
                loadOptions.LoadWith<AssemblyComponent>(ac => ac.Component);
                loadOptions.LoadWith<Assembly>(a => a.Reviews);
                loadOptions.LoadWith<Assembly>(a => a.UserVotes);

                using (var context = new AppDbContext())
                {
                    context.LoadOptions = loadOptions;
                    var updatedAssembly = context.Assemblies
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
                                updatedAssembly.OnPropertyChanged(nameof(Assembly.TotalPrice));
                                Assemblies.Insert(index, updatedAssembly);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public void DeleteAssembly(int assemblyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Настройка загрузки связанных данных
                    var loadOptions = new DataLoadOptions();
                    loadOptions.LoadWith<Assembly>(a => a.AssemblyComponents);
                    loadOptions.LoadWith<Assembly>(a => a.Reviews);
                    loadOptions.LoadWith<Assembly>(a => a.UserVotes);
                    context.LoadOptions = loadOptions;

                    // Получаем сборку со всеми зависимостями
                    var assembly = context.Assemblies
                        .FirstOrDefault(a => a.AssemblyID == assemblyId);

                    if (assembly != null)
                    {
                        // Удаляем все компоненты сборки
                        if (assembly.AssemblyComponents != null && assembly.AssemblyComponents.Any())
                        {
                            context.AssemblyComponents.DeleteAllOnSubmit(assembly.AssemblyComponents);
                            context.SubmitChanges(); // Сохраняем удаление компонентов
                        }

                        // Удаляем все отзывы
                        if (assembly.Reviews != null && assembly.Reviews.Any())
                        {
                            context.Reviews.DeleteAllOnSubmit(assembly.Reviews);
                            context.SubmitChanges(); // Сохраняем удаление отзывов
                        }

                        // Удаляем все голоса
                        if (assembly.UserVotes != null && assembly.UserVotes.Any())
                        {
                            context.UserVotes.DeleteAllOnSubmit(assembly.UserVotes);
                            context.SubmitChanges(); // Сохраняем удаление голосов
                        }

                        // Теперь можно удалить саму сборку
                        context.Assemblies.DeleteOnSubmit(assembly);
                        context.SubmitChanges();

                        // Обновляем UI
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var item = Assemblies.FirstOrDefault(a => a.AssemblyID == assemblyId);
                            if (item != null)
                                Assemblies.Remove(item);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ShowReviews(int assemblyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var loadOptions = new DataLoadOptions();
                    loadOptions.LoadWith<Assembly>(a => a.Reviews);
                    loadOptions.LoadWith<Review>(r => r.User);
                    context.LoadOptions = loadOptions;

                    var assembly = context.Assemblies
                        .FirstOrDefault(a => a.AssemblyID == assemblyId);

                    if (assembly != null)
                    {
                        var reviewWindow = new ReviewsWindow(assembly, CurrentUserEmail, _dbContext);
                        reviewWindow.ShowDialog();
                        LoadAssemblies();
                    }
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

        private void UpdateCollection(System.Collections.Generic.List<Assembly> sortedAssemblies)
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