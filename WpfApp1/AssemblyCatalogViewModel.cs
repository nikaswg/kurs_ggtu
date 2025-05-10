using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MyApp.BusinessLogic;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;  

namespace MyApp.WPF
{
    public class AssemblyCatalogViewModel : INotifyPropertyChanged
    {
        private readonly AssemblyQueryService _assemblyQueryService;
        private readonly AppDbContext _dbContext;
        public ObservableCollection<Assembly> Assemblies { get; set; }

        public AssemblyCatalogViewModel(AppDbContext dbContext)
        {
            _assemblyQueryService = new AssemblyQueryService(dbContext);
            Assemblies = new ObservableCollection<Assembly>();
            LoadAssembliesAsync();
            _dbContext = new AppDbContext();
        }

        private async void LoadAssembliesAsync()
        {
            var assemblies = await _assemblyQueryService.GetAllAssembliesWithDetailsAsync();
            foreach (var assembly in assemblies)
            {
                Assemblies.Add(assembly);
            }
        }

        public void LikeAssembly(Assembly assembly, string userEmail)
        {
            try
            {
                assembly.Like(userEmail);             
                OnPropertyChanged(nameof(Assemblies));
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оценке сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DislikeAssembly(Assembly assembly, string userEmail)
        {
            try
            {
                assembly.Dislike(userEmail);
                OnPropertyChanged(nameof(Assemblies));
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при снятии оценки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}