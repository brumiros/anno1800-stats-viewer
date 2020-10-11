using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using abacus.core;
using monocle;

namespace abacus.desktop
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<IslandViewModel> _islands = new ObservableCollection<IslandViewModel>();
        private IslandViewModel _selectedIsland;

        public ObservableCollection<IslandViewModel> Islands
        {
            get => _islands;
            set
            {
                _islands = value;
                OnPropertyChanged();
            }
        }

        public IslandViewModel SelectedIsland
        {
            get => _selectedIsland;
            set
            {
                _selectedIsland = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            OnDataRefreshed(new List<IslandRawDetails>
            {
                new IslandRawDetails
                {
                    IslandData = new IslandData { id = 123, name = "island 1"},
                    ConsumptionNodes = new List<ConsumptionNode>
                    {
                        new ConsumptionNode { rate = 1.23452, resourceType = Resource.Fish },
                        new ConsumptionNode { rate = 34.23452, resourceType = Resource.Schnapps },
                        new ConsumptionNode { rate = 0.12344, resourceType = Resource.BasicClothes }
                    },
                    ProductionNodes = new List<ProductionNode>
                    {
                        new ProductionNode { output = Resource.Fish, rate = 1.2 },
                        new ProductionNode { output = Resource.Schnapps, rate = 2.2 },
                        new ProductionNode { output = Resource.BasicClothes, rate = 3.2 }
                    }
                },
                new IslandRawDetails
                {
                    IslandData = new IslandData { id = 123, name = "island 2"},
                    ConsumptionNodes = new List<ConsumptionNode>
                    {
                        new ConsumptionNode { rate = 1.23452, resourceType = Resource.Fish },
                        new ConsumptionNode { rate = 34.23452, resourceType = Resource.Schnapps },
                        new ConsumptionNode { rate = 0.12344, resourceType = Resource.BasicClothes }
                    },
                    ProductionNodes = new List<ProductionNode>
                    {
                        new ProductionNode { output = Resource.Fish, rate = 3.2 },
                        new ProductionNode { input = new List<Resource> { Resource.Potatoes }, output = Resource.Schnapps, rate = 5.2 },
                        new ProductionNode { input = new List<Resource> { Resource.Wool }, output = Resource.BasicClothes, rate = 6.2 }
                    }
                }
            });
            // var telegraphService = new TelegraphService();
            // telegraphService.IslandDetailsObservable
            //     .ObserveOnDispatcher()
            //     .Subscribe(OnDataRefreshed);
        }

        private void OnDataRefreshed(IEnumerable<IslandRawDetails> islandDetails)
        {
            var newIslandViewModels = islandDetails.Select(ToIslandViewModel).ToList();
            var newIslandNames = newIslandViewModels.Select(vm => vm.Name);
            var islandsToDelete = Islands.Where(vm => !newIslandNames.Contains(vm.Name)).ToList();
            foreach (var islandViewModel in newIslandViewModels)
            {
                var existingViewModel = Islands.FirstOrDefault(i => i.Name == islandViewModel.Name);
                if (existingViewModel != null)
                {
                    existingViewModel.Goods = islandViewModel.Goods;
                }
                else
                {
                    Islands.Add(islandViewModel);
                }
            }

            foreach (var viewModel in islandsToDelete)
            {
                Islands.Remove(viewModel);
            }
        }

        private static IslandViewModel ToIslandViewModel(IslandRawDetails island)
        {
            var goodViewModelPerResource = new ConcurrentDictionary<Resource, IslandGoodViewModel>();

            foreach (var consumptionNode in island.ConsumptionNodes)
            {
                goodViewModelPerResource[consumptionNode.resourceType] = new IslandGoodViewModel
                {
                    Name = consumptionNode.resourceType.ToString(),
                    Demand = consumptionNode.rate
                };
            }

            foreach (var productionNode in island.ProductionNodes)
            {
                var goodViewModel = goodViewModelPerResource.GetOrAdd(productionNode.output, resource => new IslandGoodViewModel
                {
                    Name = resource.ToString()
                });

                goodViewModel.Supply += productionNode.rate;

                foreach (var inputResource in productionNode.input ?? new List<Resource>())
                {
                    var consumptionGoodViewModel = goodViewModelPerResource.GetOrAdd(inputResource, resource => new IslandGoodViewModel
                    {
                        Name = resource.ToString()
                    });
                    consumptionGoodViewModel.Demand += productionNode.rate;
                }
            }

            return new IslandViewModel
            {
                Name = island.IslandData.name,
                Goods = new ObservableCollection<IslandGoodViewModel>(goodViewModelPerResource.Values)
            };
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
