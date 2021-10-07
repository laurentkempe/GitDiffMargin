using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitDiffMargin.ViewModel
{
    internal abstract class ViewModelBase : INotifyPropertyChanged

    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Cleanup()
        {
        }
    }
}