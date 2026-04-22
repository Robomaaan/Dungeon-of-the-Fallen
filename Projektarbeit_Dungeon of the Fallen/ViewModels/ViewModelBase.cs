using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    /// <summary>
    /// Basis-ViewModel mit INotifyPropertyChanged für MVVM-Datenbindung
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
