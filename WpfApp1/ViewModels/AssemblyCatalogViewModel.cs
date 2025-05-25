using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
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
            LoadAssemblies();
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

        private void LoadAssemblies() // Убрали async, так как LINQ to SQL не поддерживает асинхронные операции
        {
            try
            {
                // Настраиваем загрузку связанных данных
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Assembly>(a => a.User);
                loadOptions.LoadWith<Assembly>(a => a.AssemblyComponents);
                loadOptions.LoadWith<AssemblyComponent>(ac => ac.Component);
                loadOptions.LoadWith<Assembly>(a => a.Reviews);
                loadOptions.LoadWith<Review>(r => r.User);
                loadOptions.LoadWith<Assembly>(a => a.UserVotes);

                _dbContext.LoadOptions = loadOptions;

                // Получаем данные
                var assemblies = _dbContext.Assemblies.ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Assemblies.Clear();
                    foreach (var assembly in assemblies)
                    {
                        // Для каждого Assembly вручную вычисляем TotalPrice
                        assembly.OnPropertyChanged(nameof(Assembly.TotalPrice)); 

                        Assemblies.Add(assembly);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сборок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _dbContext.LoadOptions = null; // Сбрасываем LoadOptions
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

                // Получаем сборку по ID
                var assembly = _dbContext.Assemblies
                    .FirstOrDefault(a => a.AssemblyID == assemblyId);

                if (assembly != null)
                {
                    // Увеличиваем рейтинг
                    assembly.Rating++;

                    // Добавляем запись о голосовании
                    _dbContext.UserVotes.InsertOnSubmit(new UserVote
                    {
                        AssemblyID = assemblyId,
                        UserEmail = _currentUserEmail,
                        VoteType = 1 // 1 для лайка
                    });

                    // Сохраняем изменения
                    _dbContext.SubmitChanges();

                    // Обновляем UI
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

                // Получаем сборку из базы
                var assembly = _dbContext.Assemblies
                    .FirstOrDefault(a => a.AssemblyID == assemblyId);

                if (assembly != null)
                {
                    // Уменьшаем рейтинг
                    assembly.Rating--;

                    // Добавляем голос пользователя
                    _dbContext.UserVotes.InsertOnSubmit(new UserVote
                    {
                        AssemblyID = assemblyId,
                        UserEmail = _currentUserEmail,
                        VoteType = 0 // 0 для дизлайка
                    });

                    // Сохраняем изменения и обновляем данные
                    _dbContext.SubmitChanges();
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
            _dbContext.SubmitChanges();
            RefreshAssembly(assemblyId);
        }

        private void RefreshAssembly(int assemblyId)
        {
            try
            {
                // Настраиваем загрузку связанных данных
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Assembly>(a => a.User);
                loadOptions.LoadWith<Assembly>(a => a.AssemblyComponents);
                loadOptions.LoadWith<AssemblyComponent>(ac => ac.Component);
                loadOptions.LoadWith<Assembly>(a => a.Reviews);
                loadOptions.LoadWith<Assembly>(a => a.UserVotes);

                _dbContext.LoadOptions = loadOptions;

                var updatedAssembly = _dbContext.Assemblies
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

                            // Уведомляем об обновлении TotalPrice
                            updatedAssembly.OnPropertyChanged(nameof(Assembly.TotalPrice));

                            Assemblies.Insert(index, updatedAssembly);
                        }
                    });
                }
            }
            finally
            {
                _dbContext.LoadOptions = null;
            }
        }

        private void DeleteAssembly(int assemblyId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту сборку?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Загружаем сборку с связанными данными
                var options = new DataLoadOptions();
                options.LoadWith<Assembly>(a => a.AssemblyComponents);
                options.LoadWith<Assembly>(a => a.Reviews);
                options.LoadWith<Assembly>(a => a.UserVotes);
                _dbContext.LoadOptions = options;

                var assembly = _dbContext.Assemblies
                    .FirstOrDefault(a => a.AssemblyID == assemblyId);

                if (assembly != null)
                {
                    // Удаление связанных данных
                    _dbContext.AssemblyComponents.DeleteAllOnSubmit(assembly.AssemblyComponents);
                    _dbContext.Reviews.DeleteAllOnSubmit(assembly.Reviews);
                    _dbContext.UserVotes.DeleteAllOnSubmit(assembly.UserVotes);

                    // Удаление самой сборки
                    _dbContext.Assemblies.DeleteOnSubmit(assembly);

                    // Сохранение изменений
                    _dbContext.SubmitChanges();

                    // Обновление UI
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
            finally
            {
                // Сбрасываем LoadOptions
                _dbContext.LoadOptions = null;
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
                    LoadAssemblies();
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