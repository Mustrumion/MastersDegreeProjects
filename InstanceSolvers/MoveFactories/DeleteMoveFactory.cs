﻿using InstanceGenerator;
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
    public class DeleteMoveFactory : IMoveFactory
    {
        private IEnumerable<TvBreak> _breaks;
        private Solution _solution;

        public IEnumerable<TvBreak> Breaks { get; set; }
        public Random Random { get; set; }
        public bool MildlyRandomOrder { get; set; }
        public Instance Instance { get; set; }

        public int PositionsCountLimit { get; set; }
        public bool AlwaysReturnStartsAndEnds { get; set; } = true;
        public int MaxBreaksChecked { get; set; }

        public DeleteMoveFactory()
        {
        }

        public DeleteMoveFactory(Solution solution)
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
        }

        private void Reorder()
        {
            if (MildlyRandomOrder)
            {
                _breaks = _breaks.ToList();
                (_breaks as IList<TvBreak>).Shuffle(Random);
            }
        }

        private void Select()
        {
            if (MaxBreaksChecked > 0)
            {
                _breaks = _breaks.Take(MaxBreaksChecked);
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
            return GenerateDeleteMoves();
        }


        public IEnumerable<Delete> GenerateDeleteMoves()
        {
            PrepareStructures();
            foreach (var tvBreak in _breaks)
            {
                BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
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
                    yield return new Delete()
                    {
                        Solution = Solution,
                        Instance = Instance,
                        Position = position,
                        TvBreak = tvBreak
                    };
                }
            }
        }
    }
}