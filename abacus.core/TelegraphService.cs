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
                .Interval(TimeSpan.FromSeconds(2))
                .Select(_ => GetIslandDetails());
        }

        public List<IslandRawDetails> GetIslandDetails()
        {
            if (!_telegraph.GetAllIslands(out var allIslands))
            {
                Console.WriteLine("Couldn't find any islands");
                return new List<IslandRawDetails>();
            }

            allIslands = allIslands
                .Where(island => island.name.Length > 0 && island.name.Length < 20)
                .OrderBy(i => i.name)
                .ToList();
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
                var productionBuildings = RDAHelper.GetProductionBuildings();

                var productionNodes = new List<ProductionNode>();
                foreach (var building in validBuildings)
                {
                    if (!productionBuildings.Contains(building.buidlingType))
                    {
                        continue;
                    }
                    if (!_telegraph.GetBuildingProduction(island.id, building.id, out var productionNode))
                        continue;
                    productionNodes.Add(productionNode);
                    // Console.WriteLine($"Building {building.id} - {building.buidlingType.ToString()} = {productionNode.output} @ {productionNode.rate}");
                }

                allIslandsDetails.Add(new IslandRawDetails
                {
                    IslandData = island,
                    ConsumptionNodes = consumption,
                    ProductionNodes = productionNodes
                });
            }

            // Console.WriteLine("Loaded all islands");

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
