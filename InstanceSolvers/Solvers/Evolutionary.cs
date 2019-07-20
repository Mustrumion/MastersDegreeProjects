using InstanceGenerator;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
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
        public int PopulationCount { get; set; } = 100;
        public int NumberOfTransformations { get; set; } = 10;
        public int NumberOfBreaksCrossed { get; set; } = 5;
        public int NumberOfMutants { get; set; } = 20;
        public int NumberOfCrossbreeds { get; set; } = 10;
        public bool ParallelAllowed { get; set; }

        /// <summary>
        /// Those may be used in parallel, so a single one won't cut it.
        /// </summary>
        public Func<ISolver> GenerationCreatorCreator { get; set; }

        /// <summary>
        /// Those may be used in parallel, so a single one won't cut it.
        /// </summary>
        public Func<ISolver> GenerationImproverCreator { get; set; }

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

        private void AddSolutionToGeneration(int seed)
        {
            ISolver generationCreator = GenerationCreatorCreator();
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
            lock (_generation)
            {
                _generation.Add(generationCreator.Solution);
            }
        }

        private void ImprovePopulation()
        {
            if (GenerationImproverCreator == null) return;
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
            ISolver generationImprover = GenerationImproverCreator();
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
            FillPopulation();
            while(CurrentTime.Elapsed < TimeLimit)
            {
                CreateCrossbreeds();
                CreateMutants();
                ImprovePopulation();
                CullPopulation();
                SaveTheBest();
            }
        }

        private void CreateMutants()
        {
            _generation.Shuffle(Random);
            var bases = _generation.Take(NumberOfMutants);
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
            var mutant = _solution.TakeSnapshot();
            mutant.RestoreStructures();
            mutant.GradingFunction = ScoringFunction.GetAnotherOne();
            mutant.GradingFunction.AssesSolution(mutant);


        }
        
        private void CreateCrossbreed(Solution mainSolution, Solution breakDonor)
        {
            var schedules = breakDonor.AdvertisementsScheduledOnBreaks.Values.ToList();
            schedules.Shuffle(Random);

            var kiddo = _solution.DeepCopy();
        }

        private void CreateCrossbreeds()
        {
            throw new NotImplementedException();
        }

        private void SaveTheBest()
        {
            var populationBest = _generation.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).First();
            if (populationBest.IsBetterThan(_bestSolution))
            {
                _bestSolution = populationBest.TakeSnapshot();
            }
        }

        private void CullPopulation()
        {
            _generation = _generation.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).Take(PopulationCount).ToList();
        }
    }
}
