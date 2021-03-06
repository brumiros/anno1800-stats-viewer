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
                .ObserveOnDispatcher()
                .AutoRefresh()
                .ToCollection()
                .Subscribe(UpdateSelectedIsland);

            // UseDevData();
            var telegraphService = new TelegraphService();
            telegraphService.IslandDetailsObservable.ObserveOnDispatcher().Subscribe(OnDataRefreshed);
        }

        private void UseDevData()
        {
            Observable.Interval(TimeSpan.FromSeconds(1)).ObserveOnDispatcher().Subscribe(x =>
            {
                OnDataRefreshed(new List<IslandRawDetails>
                {
                    new IslandRawDetails
                    {
                        IslandData = new IslandData { id = 123, name = "island 1" },
                        ConsumptionNodes = new List<ConsumptionNode>
                        {
                            new ConsumptionNode { rate = 1.23452 * x, resourceType = Resource.Fish },
                            new ConsumptionNode { rate = 34.23452, resourceType = Resource.Schnapps },
                            new ConsumptionNode { rate = 0.12344 * x, resourceType = Resource.WorkClothes },
                            new ConsumptionNode { rate = 0.12344, resourceType = Resource.Beer },
                            new ConsumptionNode { rate = 0.6, resourceType = Resource.Champagne },
                            new ConsumptionNode { rate = 0.312344, resourceType = Resource.FriedPlantains },
                            new ConsumptionNode { rate = 5.12344, resourceType = Resource.SewingMachines },
                            new ConsumptionNode { rate = 7.12344, resourceType = Resource.Chocolate },
                            new ConsumptionNode { rate = 8, resourceType = Resource.Oil }
                        },
                        ProductionNodes = new List<ProductionNode>
                        {
                            new ProductionNode { output = Resource.Fish, rate = 1.2 },
                            new ProductionNode { output = Resource.Schnapps, rate = 2.2 + x },
                            new ProductionNode { output = Resource.WorkClothes, rate = 3.2 }
                        }
                    },
                    new IslandRawDetails
                    {
                        IslandData = new IslandData { id = 124, name = "island 2" },
                        ConsumptionNodes = new List<ConsumptionNode>
                        {
                            new ConsumptionNode { rate = 1.23452 * x, resourceType = Resource.Fish },
                            new ConsumptionNode { rate = 34.23452, resourceType = Resource.Schnapps },
                            new ConsumptionNode { rate = 0.12344, resourceType = Resource.WorkClothes }
                        },
                        ProductionNodes = new List<ProductionNode>
                        {
                            new ProductionNode { output = Resource.Fish, rate = 3.2 },
                            new ProductionNode { input = new List<Resource> { Resource.Potatoes }, output = Resource.Schnapps, rate = 5.2 },
                            new ProductionNode { input = new List<Resource> { Resource.Wool }, output = Resource.WorkClothes, rate = 6.2 * x }
                        }
                    }
                });
            });
        }

        private void UpdateSelectedIsland(IReadOnlyCollection<IslandViewModel> islands)
        {
            var selectedIslands = islands.Where(i => i.IsSelected).ToList();

            var mergedGoods =
                IslandGoodViewModel.MergeGoodViewModels(selectedIslands.SelectMany(i => i.Goods))
                    .Where(good => good.Demand > 0 || good.Supply > 0);
            IslandsSelection = new IslandViewModel
            {
                Id = 0,
                Name = string.Join(", ", selectedIslands.Select(i => i.Name)),
                Goods = new ObservableCollection<IslandGoodViewModel>(mergedGoods)
            };
        }

        private void OnDataRefreshed(IEnumerable<IslandRawDetails> islandDetails)
        {
            // Console.WriteLine("Got data");
            var newIslandViewModels = islandDetails.Select(IslandViewModel.FromRawDetails).ToList();
            var islandsToRemove = _islandsSourceCache.Items.Where(
                oldVm => !newIslandViewModels.Any(newVm => newVm.Id == oldVm.Id));
            _islandsSourceCache.Edit(innerCache =>
            {
                foreach (var viewModel in newIslandViewModels)
                {
                    var existingViewModel = innerCache.Lookup(viewModel.Id);
                    if (existingViewModel.HasValue)
                    {
                        existingViewModel.Value.Goods = viewModel.Goods;
                        innerCache.Refresh(existingViewModel.Value);
                    }
                    else
                    {
                        innerCache.AddOrUpdate(viewModel);
                    }
                }
            });
            foreach (var islandViewModel in islandsToRemove)
            {
                _islandsSourceCache.Remove(islandViewModel);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
