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
            int limit = Solution.AdvertisementsScheduledOnBreaks.Sum(b => b.Value.Count) / 4;
            limit = Math.Min(limit, MovesReturned);
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.Where(b => b.Count > 0).ToList();
            if (breakList.Count == 0) yield break;
            for (int i = 0; i < limit; i++)
            {
                var delete = new Delete()
                {
                    Instance = Instance,
                    Solution = Solution,
                    TvBreak = breakList[Random.Next() % breakList.Count].BreakData,
                };
                delete.Position = Random.Next() % (Solution.AdvertisementsScheduledOnBreaks[delete.TvBreak.ID].Count);
                yield return delete;
            }
        }

        protected override void ChangeParametersBy(int step)
        {
            int newCount = MovesReturned + step * 8;
            MovesReturned = Math.Min(Math.Max(_minMovesReturned, newCount), MaxMovesReturned);
        }
        
    }
}
