using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace abacus.core
{
    public class IslandGoodViewModel : INotifyPropertyChanged
    {
        private string _name;
        private double _demand;
        private double _supply;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public double Demand
        {
            get => _demand;
            set
            {
                _demand = value;
                OnPropertyChanged();
            }
        }

        public double Supply
        {
            get => _supply;
            set
            {
                _supply = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
