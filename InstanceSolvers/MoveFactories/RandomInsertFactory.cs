using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstanceGenerator.Interfaces;
using InstanceSolvers.Moves;

namespace InstanceSolvers.MoveFactories
{
    public class RandomInsertFactory : BaseMoveFactory, IMoveFactory
    {
        public int MovesReturned { get; set; } = 1;
        private int _minMovesReturned = 1;

        public IEnumerable<IMove> GenerateMoves()
        {
            var ordersList = Instance.AdOrders.Values.ToList();
            var breakList = Instance.Breaks.Values.ToList();
            for (int i = 0; i < MovesReturned; i++)
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
            int newCount = MovesReturned + step * 20;
            MovesReturned = Math.Min(_minMovesReturned, newCount);
        }
        
    }
}
