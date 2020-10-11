using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using monocle;

namespace abacus.core
{
    public class IslandViewModel : INotifyPropertyChanged
    {
        private string _name;
        private ObservableCollection<IslandGoodViewModel> _goods;
        private bool _isSelected;

        public ulong Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IslandGoodViewModel> Goods
        {
            get => _goods;
            set
            {
                _goods = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"Island {Id} - {Name}";
        }

        public static IslandViewModel FromRawDetails(IslandRawDetails island)
        {
            var consumptionGoods = island.ConsumptionNodes.Select(node =>
                new IslandGoodViewModel(node.resourceType, demand: node.rate));

            var producedGoods = island.ProductionNodes.SelectMany(node =>
            {
                var result = new List<IslandGoodViewModel>
                {
                    new IslandGoodViewModel(node.output, node.rate)
                };

                result.AddRange((node.input ?? new List<Resource>()).Select(input =>
                    new IslandGoodViewModel(input, demand: node.rate)));

                return result;
            });

            var goods = IslandGoodViewModel.MergeGoodViewModels(consumptionGoods.Concat(producedGoods));
            return new IslandViewModel
            {
                Id = island.IslandData.id,
                Name = island.IslandData.name,
                Goods = new ObservableCollection<IslandGoodViewModel>(goods)
            };
        }
    }
}
