using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Linq.Mapping;

[Table(Name = "Users")] // Указываем имя таблицы в БД
public class User
{
    [Column] public string? Name { get; set; }
    [Column] public string? Email { get; set; }
    [Column(Name = "Password")] // Если в БД поле называется "Password", а не "PasswordHash"
    public string? PasswordHash { get; set; }
    [Column] public string? Role { get; set; }
    [Column] public int? NameId { get; set; }
}