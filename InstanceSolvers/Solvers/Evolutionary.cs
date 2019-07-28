using InstanceGenerator;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Solvers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class Evolutionary : BaseSolver
    {
        public int PopulationCount { get; set; } = 30;
        public int NumberOfMutatingTransformations { get; set; } = 5;
        public int NumberOfBreaksCrossed { get; set; } = 4;
        public int NumberOfMutants { get; set; } = 20;
        public int NumberOfCrossbreeds { get; set; } = 20;
        public bool ParallelAllowed { get; set; } = true;
        public int BreakAfterLoopsWithoutImprovement { get; set; }

        public int GenerationsWithoutImprovement { get; private set; }
        public int Generations { get; private set; }


        public Func<List<IMoveFactory>> MutationFactoriesGenerator { get; set; }

        /// <summary>
        /// Those may be used in parallel, so a single one won't cut it.
        /// </summary>
        public Func<ISolver> GenerationCreatorGenerator { get; set; }

        /// <summary>
        /// Those may be used in parallel, so a single one won't cut it.
        /// </summary>
        public Func<ISolver> GenerationImproverGenerator { get; set; }

        private List<Solution> _generation = new List<Solution>();
        private Solution _bestSolution;

        private void FillPopulation()
        {
            List<int> seeds = new List<int>();
            for(int i = 0; i < PopulationCount - _generation.Count; i++)
            {
                seeds.Add(Random.Next());
            }
            if (ParallelAllowed)
            {
                Parallel.ForEach(seeds, seed =>
                {
                    AddSolutionToGeneration(seed);
                });
            }
            else
            {
                foreach(int seed in seeds)
                {
                    AddSolutionToGeneration(seed);
                }
            }
        }


        private void InitializeMoveFactories()
        {
            if (MutationFactoriesGenerator == null)
            {
                MutationFactoriesGenerator = () => new List<IMoveFactory>
                {
                    new RandomDeleteFactory()
                    {
                        MovesReturned = 1,
                    },
                    new RandomInsertFactory()
                    {
                        MovesReturned = 1,
                    },
                    new RandomSwapFactory()
                    {
                        MovesReturned = 1,
                    },
                };
            }
        }

        private void AddSolutionToGeneration(int seed)
        {
            ISolver generationCreator = GenerationCreatorGenerator();
            generationCreator.Instance = Instance;
            if (PropagateRandomSeed)
            {
                generationCreator.Seed = seed;
            }
            else
            {
                generationCreator.Seed = (seed + new Random().Next()) % int.MaxValue;
            }
            generationCreator.PropagateRandomSeed = PropagateRandomSeed;
            generationCreator.ScoringFunction = ScoringFunction.GetAnotherOne();
            generationCreator.Solve();
            generationCreator.Solution.Description = $"Initial, seed:{seed}";
            lock (_generation)
            {
                _generation.Add(generationCreator.Solution);
            }
        }

        private void ImprovePopulation()
        {
            if (GenerationImproverGenerator == null) return;
            if (ParallelAllowed)
            {
                Parallel.ForEach(_generation, solution =>
                {
                    ImproveSolution(solution);
                });
            }
            else
            {
                foreach (Solution solution in _generation)
                {
                    ImproveSolution(solution);
                }
            }
        }

        private void ImproveSolution(Solution solution)
        {
            ISolver generationImprover = GenerationImproverGenerator();
            generationImprover.Instance = Instance;
            lock (Random)
            {
                if (PropagateRandomSeed)
                {
                    generationImprover.Seed = Random.Next();
                }
                else
                {
                    generationImprover.Seed = (Random.Next() + new Random().Next()) % int.MaxValue;
                }
            }
            generationImprover.PropagateRandomSeed = PropagateRandomSeed;
            generationImprover.ScoringFunction = ScoringFunction.GetAnotherOne();
            generationImprover.Solve();
        }

        protected override void InternalSolve()
        {
            InitializeMoveFactories();
            FillPopulation();
            while(!TimeToEnd())
            {
                CreateCrossbreeds();
                CreateMutants();
                ImprovePopulation();
                if (DiagnosticMessages) OutputGenScore();
                CullPopulation();
                SaveTheBest();
                Generations += 1;
                if (DiagnosticMessages) Console.WriteLine($"Generation {Generations} finished. Completion {_bestSolution.CompletionScore}. Weighted loss {_bestSolution.WeightedLoss}.");
            }
        }


        private void OutputGenScore()
        {
            _generation = _generation.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).ToList();
            for(int i = 0; i < _generation.Count; i ++)
            {
                Console.WriteLine($"No. {i}. Comp {_generation[i].CompletionScore}. Weighted {_generation[i].WeightedLoss}. Desc {_generation[i].Description}.");
            }
        }

        private bool TimeToEnd(bool outer = true)
        {
            if (CurrentTime.Elapsed > TimeLimit)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Timeout of {TimeLimit}.");
                return true;
            }
            if (BreakAfterLoopsWithoutImprovement != 0 && GenerationsWithoutImprovement >= BreakAfterLoopsWithoutImprovement)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Went through {GenerationsWithoutImprovement} generations with no improvement.");
                return true;
            }
            return false;
        }

        private void CreateMutants()
        {
            _generation.Shuffle(Random);
            var bases = _generation.Take(NumberOfMutants).ToList();
            if (ParallelAllowed)
            {
                Parallel.ForEach(bases, solution =>
                {
                    GenerateMutantBasedOn(solution);
                });
            }
            else
            {
                foreach (var solution in bases)
                {
                    GenerateMutantBasedOn(solution);
                }
            }
        }

        private void GenerateMutantBasedOn(Solution solution)
        {
            var mutant = solution.DeepCopy();
            var factories = MutationFactoriesGenerator();
            foreach (var factory in factories)
            {
                lock (Random)
                {
                    if (PropagateRandomSeed) factory.Seed = Random.Next();
                    else factory.Seed = (Random.Next() + new Random().Next()) % int.MaxValue;
                }
                factory.Solution = mutant;
            }
            for (int i = 0; i < NumberOfMutatingTransformations; i++)
            {
                var moves = factories[i % factories.Count].GenerateMoves();
                var move = moves.FirstOrDefault();
                if (move != null)
                {
                    move.Execute();
                }
            }
            mutant.Description += $", m{Generations}";
            lock (_generation)
            {
                _generation.Add(mutant);
            }
        }
        
        private void CreateCrossbreed(Solution mainSolution, Solution breakDonor)
        {
            var schedules = breakDonor.AdvertisementsScheduledOnBreaks.Values.ToList();
            schedules.Shuffle(Random);
            var kiddo = mainSolution.DeepCopy();
            int breaksCrossed = Math.Min(NumberOfBreaksCrossed, Instance.Breaks.Count / 2);
            schedules = schedules.Take(breaksCrossed).ToList();
            foreach(var schedule in schedules)
            {
                kiddo.AdvertisementsScheduledOnBreaks[schedule.ID] = schedule.DeepClone();
            }
            kiddo.GradingFunction.AssesSolution(kiddo);
            kiddo.Description += $", c{Generations}";
            lock (_generation)
            {
                _generation.Add(kiddo);
            }
        }

        private void CreateCrossbreeds()
        {
            List<Solution> pairList = new List<Solution>();
            int numberLeft = NumberOfCrossbreeds * 2;
            while(numberLeft > 0)
            {
                _generation.Shuffle(Random);
                pairList.AddRange(_generation);
                numberLeft -= _generation.Count;
            }

            if (ParallelAllowed)
            {
                Parallel.For(0, NumberOfCrossbreeds, (i) =>
                {
                    CreateCrossbreed(pairList[i * 2], pairList[i * 2 + 1]);
                });
            }
            else
            {
                for(int i = 0; i< NumberOfCrossbreeds; i++)
                {
                    CreateCrossbreed(pairList[i * 2], pairList[i * 2 + 1]);
                }
            }
        }

        private void SaveTheBest()
        {
            var populationBest = _generation.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).First();
            if (populationBest.IsBetterThan(_bestSolution))
            {
                _bestSolution = populationBest.TakeSnapshot();
                GenerationsWithoutImprovement = 0;
            }
            else
            {
                GenerationsWithoutImprovement += 1;
            }
        }

        private void CullPopulation()
        {
            _generation = _generation.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).Take(PopulationCount).ToList();
        }
    }
}
