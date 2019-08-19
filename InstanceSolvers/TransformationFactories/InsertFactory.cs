using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Transformations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.TransformationFactories
{
    public class InsertFactory : BaseTransformationFactory, ITransformationFactory
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
        public int IgnoreBreaksWhenUnitOverfillAbove { get; set; }
        public bool IgnoreTasksWithCompletedViews { get; set; }
        public bool IgnoreCompletedTasks { get; set; }
        public bool AlwaysReturnStartsAndEnds { get; set; }
        
        public int PositionsCountLimit { get; set; }
        public int MaxBreaksChecked { get; set; }
        public int MaxTasksChecked { get; set; }

        public InsertFactory()
        {
        }
        
        public InsertFactory(Solution solution)
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
            if (IgnoreBreaksWhenUnitOverfillAbove > 0)
            {
                _breaks = _breaks.Where(b => Solution.AdvertisementsScheduledOnBreaks[b.ID].UnitFill - IgnoreBreaksWhenUnitOverfillAbove < b.SpanUnits);
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
            if (IgnoreCompletedTasks)
            {
                _tasks = _tasks.Where(t => Solution.AdOrdersScores[t.ID].Completed);
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


        public IEnumerable<ITransformation> GenerateMoves()
        {
            return GenerateInsertMoves();
        }


        private List<int> GeneratePossiblePositions(BreakSchedule schedule)
        {
            IEnumerable<int> positionList = null;
            if (PositionsCountLimit == 0 || !AlwaysReturnStartsAndEnds)
            {
                positionList = Enumerable.Range(0, schedule.Count + 1);
            }
            else
            {
                positionList = Enumerable.Range(1, schedule.Count);
            }
            if (MildlyRandomOrder)
            {
                positionList = positionList.ToList();
                (positionList as IList<int>).Shuffle(Random);
            }
            if (PositionsCountLimit != 0)
            {
                if (AlwaysReturnStartsAndEnds)
                {
                    if (positionList.Count() >= 2)
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
            return positionList.ToList();
        }


        public IEnumerable<Insert> GenerateInsertMoves()
        {
            int movesReturned = 0;
            PrepareStructures();
            foreach (var tvBreak in _breaks)
            {
                BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                foreach (var task in _tasks)
                {
                    var positionList = GeneratePossiblePositions(schedule);
                    foreach (int position in positionList)
                    {
                        movesReturned += 1;
                        yield return new Insert()
                        {
                            Solution = Solution,
                            Instance = Instance,
                            Position = position,
                            AdvertisementOrder = task,
                            TvBreak = tvBreak
                        };
                        if (movesReturned > MaxMovesReturned) yield break;
                    }
                }
            }
        }

        protected override void ChangeParametersBy(int step)
        {
            switch (Random.Next() % 3)
            {
                case 0:
                    PositionsCountLimit = Math.Max(PositionsCountLimit + step, _minPositionsCountLimit);
                    break;
                case 1:
                    MaxBreaksChecked = Math.Max(MaxBreaksChecked + step, _minMaxBreaksChecked);
                    break;
                case 2:
                    MaxTasksChecked = Math.Max(MaxTasksChecked + step, _minMaxTasksChecked);
                    break;
            }
        }
    }
}
