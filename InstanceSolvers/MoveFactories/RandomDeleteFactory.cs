using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstanceGenerator.Interfaces;
using InstanceSolvers.Moves;

namespace InstanceSolvers.MoveFactories
{
    public class RandomDeleteFactory : BaseMoveFactory, IMoveFactory
    {
        public int MovesReturned { get; set; } = 1;
        private int _minMovesReturned = 1;

        public IEnumerable<IMove> GenerateMoves()
        {
            var breakList = Instance.Breaks.Values.ToList();
            for (int i = 0; i < MovesReturned; i++)
            {
                var delete = new Delete()
                {
                    Instance = Instance,
                    Solution = Solution,
                    TvBreak = breakList[Random.Next() % breakList.Count],
                };
                delete.Position = Random.Next() % (Solution.AdvertisementsScheduledOnBreaks[delete.TvBreak.ID].Count);
                yield return delete;
            }
        }

        protected override void ChangeParametersBy(int step)
        {
            int newCount = MovesReturned + step * 20;
            MovesReturned = Math.Min(Math.Max(_minMovesReturned, newCount), MaxMovesReturned);
        }
        
    }
}
