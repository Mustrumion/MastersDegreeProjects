using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.MoveFactories
{
    public class InsertMoveFactory : IMoveFactory
    {
        private Solution _solution;
        private IEnumerable<TvBreak> _breaks;
        private IEnumerable<AdvertisementTask> _tasks;

        public IEnumerable<TvBreak> Breaks { get; set; }
        public IEnumerable<AdvertisementTask> Tasks { get; set; }
        public Random Random { get; set; }
        public bool MildlyRandomOrder { get; set; }
        public Instance Instance { get; set; }
        public int IgnoreWhenUnitOverfillAbove { get; set; }
        public bool IgnoreTasksWithCompletedViews { get; set; }
        public bool AlwaysReturnStartsAndEnds { get; set; } = true;

        public int PositionsCountLimit { get; set; }

        public int MaxBreaksChecked { get; set; }
        public int MaxTasksChecked { get; set; }

        public InsertMoveFactory()
        {
        }
        
        public InsertMoveFactory(Solution solution)
        {
            Solution = solution;
        }

        public Solution Solution
        {
            get => _solution;
            set
            {
                _solution = value;
                if (_solution != null && Instance != _solution.Instance)
                {
                    Instance = Solution.Instance;
                }
            }
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
                    var orderData = Solution.AdOrderData[t.ID];
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
            return GenerateInsertMoves();
        }


        public IEnumerable<Insert> GenerateInsertMoves()
        {
            PrepareStructures();
            foreach (var tvBreak in _breaks)
            {
                BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                foreach (var task in _tasks)
                {
                    IEnumerable<int> positionList = Enumerable.Range(0, schedule.Count + 1);
                    if (MildlyRandomOrder)
                    {
                        positionList = positionList.ToList();
                        (positionList as IList<int>).Shuffle(Random);
                    }
                    if (PositionsCountLimit != 0)
                    {
                        if (AlwaysReturnStartsAndEnds)
                        {
                            if (positionList.Count() > 2)
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
