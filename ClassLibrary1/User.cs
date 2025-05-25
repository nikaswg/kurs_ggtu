using MyApp.DataLayer.Models;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Runtime.CompilerServices;

[Table(Name = "Users")]
public class User : INotifyPropertyChanged
{
    private EntitySet<Review> _reviews = new EntitySet<Review>();
    private int _nameId;
    private string _name;
    private string _email;
    private string _password;
    private string _role;

    [Column(IsPrimaryKey = true, IsDbGenerated = true)]
    public int NameId
    {
        get => _nameId;
        set
        {
            _nameId = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public string Role
    {
        get => _role;
        set
        {
            _role = value;
            OnPropertyChanged();
        }
    }

    [Association(Storage = "_reviews", OtherKey = "NameID", ThisKey = "NameId")]
    public EntitySet<Review> Reviews
    {
        get => _reviews;
        set => _reviews.Assign(value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}