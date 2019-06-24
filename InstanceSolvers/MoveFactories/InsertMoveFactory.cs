using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.MoveFactories
{
    public class InsertMoveFactory
    {
        public IEnumerable<TvBreak> Breaks { get; set; }
        public IEnumerable<AdvertisementOrder> Tasks { get; set; }
        public Random Random { get; set; }
        public bool MildlyRandomOrder { get; set; }
        public Instance Instance { get; set; }
        public Solution Solution { get; set; }
        public int PositionsCountLimit { get; set; }

        public InsertMoveFactory(Solution solution)
        {
            Solution = solution;
            Instance = solution.Instance;
        }
        
        private void PrepareStructures()
        {
            if( MildlyRandomOrder && Random == null)
            {
                Random = new Random();
            }
            if (Breaks == null)
            {
                Breaks = Instance.Breaks.Values;
            }
            if (Tasks == null)
            {
                Tasks = Instance.AdOrders.Values;
            }
            if (MildlyRandomOrder)
            {
                Breaks = Breaks.ToList();
                (Breaks as IList<TvBreak>).Shuffle(Random);
                Tasks = Tasks.ToList();
                (Tasks as IList<AdvertisementOrder>).Shuffle(Random);
            }
        }


        public IEnumerable<Insert> GenerateMoves()
        {
            foreach(var tvBreak in Breaks)
            {
                BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                foreach(var task in Tasks)
                {
                    IEnumerable<int> positionList = Enumerable.Range(0, schedule.Count + 1);
                    if (MildlyRandomOrder)
                    {
                        positionList = positionList.ToList();
                        (positionList as IList<int>).Shuffle(Random);
                    }
                    if (PositionsCountLimit != 0)
                    {
                        positionList = positionList.Take(PositionsCountLimit);
                    }
                    foreach (int position in positionList)
                    {
                        yield return new Insert()
                        {
                            Solution = Solution,
                            Instance = Instance,
                            Position = position,
                            AdvertisementOrder = task,
                            TvBreak = tvBreak
                        };
                    }
                }
            }
        }
    }
}
