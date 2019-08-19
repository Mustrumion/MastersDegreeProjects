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
    public class DeleteFactory : BaseTransformationFactory, ITransformationFactory
    {
        private static int _minPositionsCountLimit = 3;
        private static int _minMaxBreaksChecked = 1;

        private IEnumerable<TvBreak> _breaks;

        [JsonIgnore]
        public IEnumerable<TvBreak> Breaks { get; set; }
        public int PositionsCountLimit { get; set; }
        public bool AlwaysReturnStartsAndEnds { get; set; }
        public int MaxBreaksChecked { get; set; }

        public DeleteFactory()
        {
        }

        public DeleteFactory(Solution solution)
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


        public IEnumerable<ITransformation> GenerateMoves()
        {
            return GenerateDeleteMoves();
        }


        public IEnumerable<Delete> GenerateDeleteMoves()
        {
            int movesReturned = 0;
            PrepareStructures();
            foreach (var tvBreak in _breaks)
            {
                BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                IEnumerable<int> positionList = Enumerable.Range(0, schedule.Count).Reverse();
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
                positionList = positionList.ToList();
                foreach (int position in positionList)
                {
                    movesReturned += 1;
                    yield return new Delete()
                    {
                        Solution = Solution,
                        Instance = Instance,
                        Position = position,
                        TvBreak = tvBreak
                    };
                    if (movesReturned > MaxMovesReturned) yield break;
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
            }
        }
    }
}
