using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using monocle;

namespace abacus.core
{
    public class IslandGoodViewModel : INotifyPropertyChanged, IEquatable<IslandGoodViewModel>
    {
        private Resource _resource;
        private double _demand;
        private double _supply;

        public string Name => _resource.ToString();

        public Resource Resource
        {
            get => _resource;
            set
            {
                _resource = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
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

        public IslandGoodViewModel()
        {
        }

        public IslandGoodViewModel(Resource resource, double supply = 0, double demand = 0)
        {
            Resource = resource;
            Supply = supply;
            Demand = demand;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ICollection<IslandGoodViewModel> MergeGoodViewModels(IEnumerable<IslandGoodViewModel> goods)
        {
            var goodViewModelPerResource = new ConcurrentDictionary<Resource, IslandGoodViewModel>();
            foreach (var good in goods)
            {
                var goodViewModel = goodViewModelPerResource.GetOrAdd(
                    good.Resource,
                    resource => new IslandGoodViewModel(resource));
                goodViewModel.Demand += good.Demand;
                goodViewModel.Supply += good.Supply;
            }
            return goodViewModelPerResource.Values;
        }

        public override string ToString()
        {
            return $"{_resource.ToString()} [IN: {Demand}, OUT: {Supply}]";
        }

        public bool Equals(IslandGoodViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _resource == other._resource && _demand.Equals(other._demand) && _supply.Equals(other._supply);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IslandGoodViewModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_resource, _demand, _supply);
        }
    }
}
