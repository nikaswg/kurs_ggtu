using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Runtime.CompilerServices;
using MyApp.DataLayer.Models;


[Table(Name = "Reviews")]
public class Review : INotifyPropertyChanged
{
    private EntityRef<User> _user = new EntityRef<User>();
    private EntityRef<Assembly> _assembly = new EntityRef<Assembly>();
    private int _reviewId;
    private int _assemblyId;
    private int _nameId;
    private decimal _rating;
    private string _comment;

    [Column(IsPrimaryKey = true, IsDbGenerated = true)]
    public int ReviewID
    {
        get => _reviewId;
        set
        {
            _reviewId = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public int AssemblyID
    {
        get => _assemblyId;
        set
        {
            _assemblyId = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public int NameID
    {
        get => _nameId;
        set
        {
            _nameId = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public decimal Rating
    {
        get => _rating;
        set
        {
            _rating = value;
            OnPropertyChanged();
        }
    }

    [Column]
    public string Comment
    {
        get => _comment;
        set
        {
            _comment = value;
            OnPropertyChanged();
        }
    }

    [Association(Storage = "_user", ThisKey = "NameID", OtherKey = "NameId", IsForeignKey = true)]
    public User User
    {
        get => _user.Entity;
        set
        {
            _user.Entity = value;
            OnPropertyChanged();
        }
    }

    [Association(Storage = "_assembly", ThisKey = "AssemblyID", OtherKey = "AssemblyID", IsForeignKey = true)]
    public Assembly Assembly
    {
        get => _assembly.Entity;
        set
        {
            _assembly.Entity = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}