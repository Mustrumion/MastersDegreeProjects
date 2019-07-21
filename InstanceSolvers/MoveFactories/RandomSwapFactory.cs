using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstanceGenerator.Interfaces;
using InstanceSolvers.Moves;

namespace InstanceSolvers.MoveFactories
{
    public class RandomSwapFactory : BaseMoveFactory, IMoveFactory
    {
        public int MovesReturned { get; set; } = 1;
        private int _minMovesReturned = 1;

        public IEnumerable<IMove> GenerateMoves()
        {
            int limit = Solution.AdvertisementsScheduledOnBreaks.Sum(b => b.Value.Count) * Instance.AdOrders.Count / 4;
            limit = Math.Min(limit, MovesReturned);
            var ordersList = Instance.AdOrders.Values.ToList();
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.Where(b => b.Count > 0).ToList();
            if (breakList.Count == 0) yield break;
            for (int i = 0; i < limit; i++)
            {
                var swap = new Swap()
                {
                    Instance = Instance,
                    Solution = Solution,
                    AdvertisementOrder = ordersList[Random.Next() % ordersList.Count],
                    TvBreak = breakList[Random.Next() % breakList.Count].BreakData,
                };
                swap.Position = Random.Next() % (Solution.AdvertisementsScheduledOnBreaks[swap.TvBreak.ID].Count);
                yield return swap;
            }
        }

        protected override void ChangeParametersBy(int step)
        {
            int newCount = MovesReturned + step * 8;
            MovesReturned = Math.Min(Math.Max(_minMovesReturned, newCount), MaxMovesReturned);
        }
        
    }
}
