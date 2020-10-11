using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using monocle;

namespace abacus.core
{
    public class TelegraphService
    {
        private readonly Telegraph _telegraph;

        public IObservable<List<IslandRawDetails>> IslandDetailsObservable { get; }

        public TelegraphService()
        {
            _telegraph = new Telegraph();
            IslandDetailsObservable = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => GetIslandDetails());
        }

        public List<IslandRawDetails> GetIslandDetails()
        {
            if (!_telegraph.GetAllIslands(out var allIslands))
            {
                Console.WriteLine("Couldn't find any islands");
                return new List<IslandRawDetails>();
            }

            allIslands = allIslands.Where(island => island.name.Length > 0 && island.name.Length < 20).ToList();
            var allIslandsDetails = new List<IslandRawDetails>();

            foreach (var island in allIslands)
            {
                // Console.WriteLine($"Found island {island.name} - {island.id}");

                if (!_telegraph.GetIslandConsumption(island.id, out var consumption))
                    continue;

                // foreach(var con in consumption)
                // {
                //     Console.WriteLine($"{con.resourceType.ToString()} - {con.rate}");
                // }

                if (!_telegraph.GetIslandBuildings(island.id, out var buildings))
                    continue;

                var validBuildings = buildings.Where(b => b.buidlingType != Building.Invalid).ToList();
                var productionBuildingsIds = new[] { Building.Bakery, Building.GrainFarm };
                var productionBuildings = validBuildings.Where(b => productionBuildingsIds.Contains(b.buidlingType)).ToList();
                // Console.WriteLine($"{buildings.Count} buildings ({validBuildings.Count} valid, {productionBuildings.Count} production)");

                var productionNodes = new List<ProductionNode>();
                foreach (var building in productionBuildings)
                {
                    // Console.WriteLine($"Building {building.id} - {building.buidlingType.ToString()}");

                    if (!_telegraph.GetBuildingProduction(island.id, building.id, out var productionNode))
                        continue;
                    productionNodes.Add(productionNode);
                    Console.WriteLine($"{productionNode.rate} {productionNode.output}");
                }

                allIslandsDetails.Add(new IslandRawDetails
                {
                    IslandData = island,
                    ConsumptionNodes = consumption,
                    ProductionNodes = productionNodes
                });
            }

            return allIslandsDetails;
        }
    }

    public class IslandRawDetails
    {
        public IslandData IslandData { get; set; }
        public List<ConsumptionNode> ConsumptionNodes { get; set; }
        public List<ProductionNode> ProductionNodes { get; set; }
    }
}
