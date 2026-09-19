using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Translumo.Utils
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void SetProperty<T>(ref T member, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, value))
            {
                return;
            }

            var previousValue = member;
            member = value;
            try
            {
                OnPropertyChanged(propertyName);
            }
            catch
            {
                member = previousValue;
                OnPropertyChanged(propertyName);
                throw;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
