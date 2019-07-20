using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.MoveFactories
{
    public class SwapMoveFactory : BaseMoveFactory, IMoveFactory
    {
        private static int _minPositionsCountLimit = 3;
        private static int _minMaxBreaksChecked = 1;
        private static int _minMaxTasksChecked = 1;

        private IEnumerable<TvBreak> _breaks;
        private IEnumerable<AdvertisementTask> _tasks;

        [JsonIgnore]
        public IEnumerable<TvBreak> Breaks { get; set; }
        [JsonIgnore]
        public IEnumerable<AdvertisementTask> Tasks { get; set; }

        public int IgnoreWhenUnitOverfillAbove { get; set; }
        public bool IgnoreTasksWithCompletedViews { get; set; }
        public bool AlwaysReturnStartsAndEnds { get; set; }

        public int PositionsCountLimit { get; set; }

        public int MaxBreaksChecked { get; set; }
        public int MaxTasksChecked { get; set; }

        public SwapMoveFactory()
        {
        }

        public SwapMoveFactory(Solution solution)
        {
            Solution = solution;
        }

        private void PrepareInitialLists()
        {
            if (MildlyRandomOrder && Random == null)
            {
                Random = new Random();
            }
            if (Breaks == null)
            {
                _breaks = Instance.Breaks.Values;
            }
            else
            {
                _breaks = Breaks;
            }
            if (IgnoreWhenUnitOverfillAbove > 0)
            {
                _breaks = _breaks.Where(b => Solution.AdvertisementsScheduledOnBreaks[b.ID].UnitFill - IgnoreWhenUnitOverfillAbove < b.SpanUnits);
            }
            if (Tasks == null)
            {
                _tasks = Instance.AdOrders.Values;
            }
            else
            {
                _tasks = Tasks;
            }
            if (IgnoreTasksWithCompletedViews)
            {
                _tasks = _tasks.Where(t =>
                {
                    var orderData = Solution.AdOrdersScores[t.ID];
                    return !orderData.ViewsSatisfied || !orderData.TimesAiredSatisfied;
                });
            }
        }

        private void Reorder()
        {
            if (MildlyRandomOrder)
            {
                _breaks = _breaks.ToList();
                (_breaks as IList<TvBreak>).Shuffle(Random);
                _tasks = _tasks.ToList();
                (_tasks as IList<AdvertisementTask>).Shuffle(Random);
            }
        }

        private void Select()
        {
            if (MaxBreaksChecked > 0)
            {
                _breaks = _breaks.Take(MaxBreaksChecked);
            }
            if (MaxTasksChecked > 0)
            {
                _tasks = _tasks.Take(MaxTasksChecked);
            }
        }

        private void PrepareStructures()
        {
            PrepareInitialLists();
            Reorder();
            Select();
        }


        public IEnumerable<IMove> GenerateMoves()
        {
            return GenerateSwapMoves();
        }


        public IEnumerable<Swap> GenerateSwapMoves()
        {
            int movesReturned = 0;
            PrepareStructures();
            foreach (var tvBreak in _breaks)
            {
                BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                foreach (var task in _tasks)
                {
                    IEnumerable<int> positionList = Enumerable.Range(0, schedule.Count);
                    if (MildlyRandomOrder)
                    {
                        positionList = positionList.ToList();
                        (positionList as IList<int>).Shuffle(Random);
                    }
                    
                    if (PositionsCountLimit != 0)
                    {
                        if (AlwaysReturnStartsAndEnds)
                        {
                            if(positionList.Count() > 2)
                            {
                                var newList = new List<int> { positionList.First(), positionList.Last() };
                                newList.AddRange(positionList.Take(PositionsCountLimit - 2));
                                positionList = newList;
                            }
                        }
                        else
                        {
                            positionList = positionList.Take(PositionsCountLimit);
                        }
                    }
                    foreach (int position in positionList)
                    {
                        movesReturned += 1;
                        yield return new Swap()
                        {
                            Solution = Solution,
                            Instance = Instance,
                            Position = position,
                            AdvertisementOrder = task,
                            TvBreak = tvBreak
                        };
                    }
                }
                if (movesReturned > MaxMovesReturned) break;
            }
        }
        
        protected override void ChangeParametersBy(int step)
        {
            PositionsCountLimit = Math.Max(PositionsCountLimit - step, _minPositionsCountLimit);
            MaxBreaksChecked = Math.Max(MaxBreaksChecked - step, _minMaxBreaksChecked);
            MaxTasksChecked = Math.Max(MaxTasksChecked - step, _minMaxTasksChecked);
        }
    }
}
