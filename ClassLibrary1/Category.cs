using System.ComponentModel;
using System.Data.Linq.Mapping;

[Table(Name = "Category")]
public class Category : INotifyPropertyChanged
{
    [Column(IsPrimaryKey = true, IsDbGenerated = true)]
    public int CategoryID { get; set; }

    [Column]
    public string Name { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}