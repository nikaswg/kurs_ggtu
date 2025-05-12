using System.Linq;
using System.Windows;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using System.Windows.Controls;

namespace MyApp.WPF
{
    public partial class ReviewsWindow : Window
    {
        private readonly Assembly _assembly;
        private readonly string _currentUserEmail;
        private readonly AppDbContext _dbContext;

        public string AssemblyInfo => $"Отзывы о сборке #{_assembly.AssemblyID}";
        public System.Collections.IEnumerable Reviews => _assembly.Reviews.Select(r => new
        {
            UserEmail = r.User?.Email ?? "Guest",
            r.Rating,
            r.Comment
        });

        public ReviewsWindow(Assembly assembly, string currentUserEmail, AppDbContext dbContext)
        {
            InitializeComponent();
            _assembly = assembly;
            _currentUserEmail = currentUserEmail;
            _dbContext = dbContext;
            DataContext = this;
        }

        private void AddReview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RatingComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(CommentTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, введите оценку и комментарий", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем выбранный рейтинг
                var ratingItem = (ComboBoxItem)RatingComboBox.SelectedItem;
                var rating = decimal.Parse(ratingItem.Content.ToString());

                var comment = CommentTextBox.Text;

                // Находим пользователя
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == _currentUserEmail);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем существование сборки
                if (!_dbContext.Assemblies.Any(a => a.AssemblyID == _assembly.AssemblyID))
                {
                    MessageBox.Show("Сборка не найдена", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаем отзыв
                var review = new Review
                {
                    AssemblyID = _assembly.AssemblyID,
                    NameID = user.NameId, // Убедитесь, что это свойство существует в User
                    Rating = rating,
                    Comment = comment
                };

                // Добавляем отзыв
                _dbContext.Reviews.Add(review);
                _dbContext.SaveChanges();

                // Обновляем список отзывов

                CommentTextBox.Clear();
                RatingComboBox.SelectedIndex = 4;

                // Обновляем DataContext для отображения нового отзыва
                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отзыва: {ex.Message}\n\nInner exception: {ex.InnerException?.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}