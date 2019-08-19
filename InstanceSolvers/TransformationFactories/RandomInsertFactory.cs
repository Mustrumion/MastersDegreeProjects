using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstanceGenerator.Interfaces;
using InstanceSolvers.Transformations;

namespace InstanceSolvers.TransformationFactories
{
    public class RandomInsertFactory : BaseTransformationFactory, ITransformationFactory
    {
        public int MovesReturned { get; set; } = 1;
        private int _minMovesReturned = 1;

        public IEnumerable<ITransformation> GenerateMoves()
        {
            int limit = Solution.AdvertisementsScheduledOnBreaks.Sum(b => b.Value.Count + 1) * Instance.AdOrders.Count / 4;
            limit = Math.Min(limit, MovesReturned);
            var ordersList = Instance.AdOrders.Values.ToList();
            var breakList = Instance.Breaks.Values.ToList();
            for (int i = 0; i < limit; i++)
            {
                var insert = new Insert()
                {
                    Instance = Instance,
                    Solution = Solution,
                    AdvertisementOrder = ordersList[Random.Next() % ordersList.Count],
                    TvBreak = breakList[Random.Next() % breakList.Count],
                };
                insert.Position = Random.Next() % (Solution.AdvertisementsScheduledOnBreaks[insert.TvBreak.ID].Count + 1);
                yield return insert;
            }
        }

        protected override void ChangeParametersBy(int step)
        {
            int newCount = MovesReturned + step;
            MovesReturned = Math.Min(Math.Max(_minMovesReturned, newCount), MaxMovesReturned);
        }
        
    }
}
