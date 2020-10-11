using monocle;
using NUnit.Framework;

namespace abacus.core.tests
{
    public class IslandGoodViewModelTests
    {
        [Test]
        public void MergeGoodViewModel_GivenMultipleGoods_CombinesAsExpected()
        {
            var mergedGoods = IslandGoodViewModel.MergeGoodViewModels(new[]
            {
                new IslandGoodViewModel { Resource = Resource.Plantains ,Demand = 1, Supply = 0.5 },
                new IslandGoodViewModel { Resource = Resource.Bricks, Demand = 0, Supply = 1.3 },
                new IslandGoodViewModel { Resource = Resource.Plantains, Demand = 9.8, Supply = 0.4 }
            });

            var expectedGoods = new[]
            {
                new IslandGoodViewModel { Resource = Resource.Plantains, Demand = 10.8, Supply = 0.9 },
                new IslandGoodViewModel { Resource = Resource.Bricks, Demand = 0, Supply = 1.3 }
            };
            CollectionAssert.AreEqual(expectedGoods, mergedGoods);
        }
    }
}
