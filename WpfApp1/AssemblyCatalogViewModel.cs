using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MyApp.BusinessLogic;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;

namespace MyApp.WPF
{
    public class AssemblyCatalogViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _dbContext;
        public ObservableCollection<Assembly> Assemblies { get; set; }

        public AssemblyCatalogViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            Assemblies = new ObservableCollection<Assembly>();
            LoadAssembliesAsync();
        }

        private async void LoadAssembliesAsync()
        {
            var assemblies = await _dbContext.Assemblies
                .Include(a => a.User)
                .Include(a => a.AssemblyComponents)
                .ThenInclude(ac => ac.Component)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}