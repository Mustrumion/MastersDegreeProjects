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
            var ordersList = Instance.AdOrders.Values.ToList();
            var breakList = Instance.Breaks.Values.ToList();
            for (int i = 0; i < MovesReturned; i++)
            {
                var swap = new Swap()
                {
                    Instance = Instance,
                    Solution = Solution,
                    AdvertisementOrder = ordersList[Random.Next() % ordersList.Count],
                    TvBreak = breakList[Random.Next() % breakList.Count],
                };
                swap.Position = Random.Next() % (Solution.AdvertisementsScheduledOnBreaks[swap.TvBreak.ID].Count);
                yield return swap;
            }
        }

        protected override void ChangeParametersBy(int step)
        {
            int newCount = MovesReturned + step * 20;
            MovesReturned = Math.Min(_minMovesReturned, newCount);
        }
        
    }
}
