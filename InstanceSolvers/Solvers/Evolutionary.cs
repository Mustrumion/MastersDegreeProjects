using InstanceGenerator;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.TransformationFactories;
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
        public double NumberOfMutationsToBreakCount { get; set; } = 0.01;
        public double ProportionOfBreaksCrossed { get; set; } = 0.02;
        public int NumberOfMutants { get; set; } = 20;
        public double MutationRate
        {
            get => (double)NumberOfMutants / PopulationCount;
            set => NumberOfMutants = (int)(PopulationCount * value) + 1;
        }
        public int NumberOfCrossbreeds { get; set; } = 20;
        public double CrossoverRate
        {
            get => (double)NumberOfCrossbreeds / PopulationCount;
            set => NumberOfCrossbreeds = (int)(PopulationCount * value) + 1;
        }
        public int CandidatesForParent { get; set; } = 2;
        public bool ParallelAllowed { get; set; } = true;
        public ParallelOptions ParallelOptions { get; set; } = new ParallelOptions();
        public int BreakAfterLoopsWithoutImprovement { get; set; } = 1;

        public int GenerationsWithoutImprovement { get; private set; }
        public int Generations { get; private set; }
        public List<Solution> Population { get; set; } = new List<Solution>();


        public Func<List<ITransformationFactory>> MutationFactoriesGenerator { get; set; }

        /// <summary>
        /// Those may be used in parallel, so a single one won't cut it.
        /// </summary>
        public Func<ISolver> GenerationCreatorGenerator { get; set; }

        /// <summary>
        /// Those may be used in parallel, so a single one won't cut it.
        /// </summary>
        public Func<ISolver> GenerationImproverGenerator { get; set; }

        private Solution _bestSolution;

        private void FillPopulation()
        {
            List<int> seeds = new List<int>();
            for(int i = 0; i < PopulationCount - Population.Count; i++)
            {
                seeds.Add(Random.Next());
            }
            if (ParallelAllowed)
            {
                Parallel.ForEach(seeds, ParallelOptions, seed =>
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
                MutationFactoriesGenerator = () => new List<ITransformationFactory>
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
            generationCreator.Solution.Description = $"Seed:{seed}";
            lock (Population)
            {
                Population.Add(generationCreator.Solution);
            }
        }

        private void ImprovePopulation()
        {
            if (GenerationImproverGenerator == null) return;
            if (ParallelAllowed)
            {
                Parallel.ForEach(Population, ParallelOptions, solution =>
                {
                    ImproveSolution(solution);
                });
            }
            else
            {
                foreach (Solution solution in Population)
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
            generationImprover.Solution = solution;
            generationImprover.Solve();
        }

        protected override void InternalSolve()
        {
            InitializeMoveFactories();
            if(Population == null || Population.Count == 0)
            {
                FillPopulation();
            }
            SaveTheBest();
            CreateGenReport();
            while (!TimeToEnd())
            {
                CreateCrossbreeds();
                CreateMutants();
                ImprovePopulation();
                if (DiagnosticMessages) OutputGenScore();
                CullPopulation();
                SaveTheBest();
                CreateGenReport();
                Generations += 1;
                if (DiagnosticMessages) Console.WriteLine($"Generation {Generations} finished. Completion {_bestSolution.CompletionScore}. Weighted loss {_bestSolution.WeightedLoss}.");
            }
            Solution = _bestSolution;
            //Solution.RestoreStructures();
            //_scoringFunction.AssesSolution(Solution);
        }


        private void OutputGenScore()
        {
            Population = Population.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).ToList();
            for (int i = 0; i < Population.Count; i++)
            {
                Console.WriteLine($"No. {i}. Comp {Population[i].CompletionScore}. Weighted {Population[i].WeightedLoss}. Desc {Population[i].Description}.");
            }
        }

        private void CreateGenReport()
        {
            Reporter.AddEntry(new ReportEntry()
            {
                Time = DateTime.Now,
                Action = "Generation",
                AttainedAcceptable = false,
                IntegrityLoss = _bestSolution.IntegrityLossScore,
                WeightedLoss = _bestSolution.WeightedLoss,
            });
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
            Population.Shuffle(Random);
            var bases = Population.Take(NumberOfMutants).ToList();
            if (ParallelAllowed)
            {
                Parallel.ForEach(bases, ParallelOptions, solution =>
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
            int numberOfMutations = Math.Max(Convert.ToInt32(Instance.Breaks.Count * NumberOfMutationsToBreakCount), 1);
            for (int i = 0; i < numberOfMutations; i++)
            {
                var moves = factories[i % factories.Count].GenerateMoves();
                var move = moves.FirstOrDefault();
                if (move != null)
                {
                    move.Execute();
                }
            }
            mutant.Description += $", m{Generations}";
            lock (Population)
            {
                Population.Add(mutant);
            }
        }
        
        private void CreateCrossbreed(IEnumerable<Solution> mainCandidates, IEnumerable<Solution> donorCandidates)
        {
            var mainSolution = mainCandidates.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).First();
            var breakDonor = donorCandidates.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).First();
            var schedules = breakDonor.AdvertisementsScheduledOnBreaks.Values.ToList();
            schedules.Shuffle(Random);
            var kiddo = mainSolution.DeepCopy();
            int numberOfBreaksCrossed = Math.Max(Convert.ToInt32(Instance.Breaks.Count * ProportionOfBreaksCrossed), 1);
            int breaksCrossed = Math.Min(numberOfBreaksCrossed, Instance.Breaks.Count / 2);
            schedules = schedules.Take(breaksCrossed).ToList();
            foreach(var schedule in schedules)
            {
                kiddo.AdvertisementsScheduledOnBreaks[schedule.ID] = schedule.DeepClone();
            }
            kiddo.GradingFunction.AssesSolution(kiddo);
            kiddo.Description += $", c{Generations}";
            lock (Population)
            {
                Population.Add(kiddo);
            }
        }

        private void CreateCrossbreeds()
        {
            List<Solution> pairList = new List<Solution>();
            int numberLeft = NumberOfCrossbreeds * 2 * CandidatesForParent;
            while(numberLeft > 0)
            {
                Population.Shuffle(Random);
                pairList.AddRange(Population);
                numberLeft -= Population.Count;
            }

            if (ParallelAllowed)
            {
                Parallel.For(0, NumberOfCrossbreeds, (i) =>
                {
                    var mainCandidates = pairList.GetRange(i * 2 * CandidatesForParent, CandidatesForParent);
                    var donorCandidates = pairList.GetRange((i * 2 + 1) * CandidatesForParent, CandidatesForParent);
                    CreateCrossbreed(mainCandidates, donorCandidates);
                });
            }
            else
            {
                for(int i = 0; i< NumberOfCrossbreeds; i++)
                {
                    var mainCandidates = pairList.GetRange(i * 2 * CandidatesForParent, CandidatesForParent);
                    var donorCandidates = pairList.GetRange((i * 2 + 1) * CandidatesForParent, CandidatesForParent);
                    CreateCrossbreed(mainCandidates, donorCandidates);
                }
            }
        }

        private void SaveTheBest()
        {
            var populationBest = Population.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).First();
            if (populationBest.IsBetterThan(_bestSolution))
            {
                _bestSolution = populationBest;
                GenerationsWithoutImprovement = 0;
            }
            else
            {
                GenerationsWithoutImprovement += 1;
            }
        }

        private void CullPopulation()
        {
            Population = Population.OrderBy(s => s.WeightedLoss).OrderBy(s => s.IntegrityLossScore).Take(PopulationCount).ToList();
        }
    }
}
