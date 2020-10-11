using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using abacus.core;
using DynamicData;
using DynamicData.Binding;
using monocle;

namespace abacus.desktop
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly SourceCache<IslandViewModel, ulong> _islandsSourceCache;
        private readonly ObservableCollectionExtended<IslandViewModel> _islands;
        private IslandViewModel _islandsSelection;

        public ObservableCollectionExtended<IslandViewModel> Islands => _islands;

        public IslandViewModel IslandsSelection
        {
            get => _islandsSelection;
            private set
            {
                _islandsSelection = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            _islands = new ObservableCollectionExtended<IslandViewModel>();
            _islandsSourceCache = new SourceCache<IslandViewModel, ulong>(vm => vm.Id);
            _islandsSourceCache
                .Connect()
                .Bind(_islands)
                .DisposeMany()
                .AutoRefresh()
                .ToCollection()
                .Subscribe(UpdateSelectedIsland);
            OnDataRefreshed(new List<IslandRawDetails>
            {
                new IslandRawDetails
                {
                    IslandData = new IslandData { id = 123, name = "island 1"},
                    ConsumptionNodes = new List<ConsumptionNode>
                    {
                        new ConsumptionNode { rate = 1.23452, resourceType = Resource.Fish },
                        new ConsumptionNode { rate = 34.23452, resourceType = Resource.Schnapps },
                        new ConsumptionNode { rate = 0.12344, resourceType = Resource.WorkClothes }
                    },
                    ProductionNodes = new List<ProductionNode>
                    {
                        new ProductionNode { output = Resource.Fish, rate = 1.2 },
                        new ProductionNode { output = Resource.Schnapps, rate = 2.2 },
                        new ProductionNode { output = Resource.WorkClothes, rate = 3.2 }
                    }
                },
                new IslandRawDetails
                {
                    IslandData = new IslandData { id = 124, name = "island 2"},
                    ConsumptionNodes = new List<ConsumptionNode>
                    {
                        new ConsumptionNode { rate = 1.23452, resourceType = Resource.Fish },
                        new ConsumptionNode { rate = 34.23452, resourceType = Resource.Schnapps },
                        new ConsumptionNode { rate = 0.12344, resourceType = Resource.WorkClothes }
                    },
                    ProductionNodes = new List<ProductionNode>
                    {
                        new ProductionNode { output = Resource.Fish, rate = 3.2 },
                        new ProductionNode { input = new List<Resource> { Resource.Potatoes }, output = Resource.Schnapps, rate = 5.2 },
                        new ProductionNode { input = new List<Resource> { Resource.Wool }, output = Resource.WorkClothes, rate = 6.2 }
                    }
                }
            });

            // var telegraphService = new TelegraphService();
            // telegraphService.IslandDetailsObservable
            //     .ObserveOnDispatcher()
            //     .Subscribe(OnDataRefreshed);
        }

        private void UpdateSelectedIsland(IReadOnlyCollection<IslandViewModel> islands)
        {
            var selectedIslands = islands.Where(i => i.IsSelected).ToList();

            IslandsSelection = new IslandViewModel
            {
                Id = 0,
                Name = string.Join(", ", selectedIslands.Select(i => i.Name)),
                Goods = new ObservableCollection<IslandGoodViewModel>(IslandGoodViewModel.MergeGoodViewModels(
                    selectedIslands.SelectMany(i => i.Goods)))
            };
        }

        private void OnDataRefreshed(IEnumerable<IslandRawDetails> islandDetails)
        {
            var newIslandViewModels = islandDetails.Select(IslandViewModel.FromRawDetails).ToList();
            _islandsSourceCache.AddOrUpdate(newIslandViewModels);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
