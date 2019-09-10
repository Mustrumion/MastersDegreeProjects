using InstanceSolvers;
using InstanceSolvers.Solvers;
using InstanceSolvers.Solvers.Base;
using InstanceSolvers.TransformationFactories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    public class EvolutionaryTuning
    {
        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver = new BulkSolver()
            {
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = false,
                ReportProgrssToFile = true,
                LengthFilter = new[] { "week.json" },
                //KindFilter = new[] { "1_ch_n", "2_ch_n", "2_ch_v", "3_ch_n", "3edu1", "3edu2", "4ch3n1a1", "4ch3n1a2" },
                DifficultyFilter = new[] { "medium" },
                StartingSolutionsDirectory = Path.Combine(MAIN_DIRECTORY, "solutions", "base_solutions_all"),
                SavedSubpath = "evolutionary_optimization"
            };
            Random random = new Random();
            
            while (true)
            {
                int population = random.Next(15, 46);
                int parentCandidates = random.Next(1, 5);
                double mutationRate = random.NextDouble();
                double crossoverRate = random.NextDouble();
                double powerForMut = random.NextDouble() * 6 + 1;
                double mutationProbability = 0.1 * Math.Pow(0.5, powerForMut);
                double powerForCros = random.NextDouble() * 6 + 1;
                double crossoverProbability = 0.5 * Math.Pow(0.5, powerForCros);
                string desc = $"{population}_{parentCandidates}_{mutationRate:0.###}_{crossoverRate:0.###}_{mutationProbability:0.#######}_{crossoverProbability:0.#######}";
                Console.WriteLine($"currently testing {desc}\n\n\n");
                bulkSolver.SolveEverything(() => EvolutionaryMakerParametrized(population, parentCandidates, mutationRate, crossoverRate, mutationProbability, crossoverProbability, desc));
            }
        }

        private ISolver EvolutionaryMakerParametrized(int population, int parentCandidates, double mutationRate, double crossoverRate, double mutationProbability, double crossoverProbability, string desc)
        {
            Evolutionary solver = new Evolutionary()
            {
                ParallelAllowed = true,
                ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 15 },
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(0, 15, 0),
                PopulationCount = population,
                CandidatesForParent = parentCandidates,
                MutationRate = mutationRate,
                CrossoverRate = crossoverRate,
                NumberOfMutationsToBreakCount = mutationProbability,
                ProportionOfBreaksCrossed = crossoverProbability,
                GenerationImproverGenerator = LocalSearchForEvolutionaryImprovement,
                Description = desc,
            };
            return solver;
        }


        private static ISolver LocalSearchForEvolutionaryImprovement()
        {
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 1,
                TimeLimit = new TimeSpan(0, 0, 15),
            };
            LocalSearch localSearch = new LocalSearch()
            {
                NumberOfNoGoodActionsToStop = 15,
                BestFactoryAdjustmentParam = 0.2,
                NeighberhoodAdjustmentParam = 0.2,
                ImprovementOverNarrowNeighb = 1.5,
                TimeLimit = new TimeSpan(0, 0, 15),
            };
            localSearch.MoveFactories = new List<ITransformationFactory>
            {
                new InsertFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 3,
                    MaxTasksChecked = 1,
                    MaxBreaksChecked = 1,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = true,
                    IgnoreTasksWithCompletedViews = false,
                    AlwaysReturnStartsAndEnds = true,
                },
                new RandomDeleteFactory()
                {
                    MovesReturned = 20,
                    RampUpSpeed = 3.0,
                },
                new RandomInsertFactory()
                {
                    MovesReturned = 20,
                    RampUpSpeed = 3.0,
                },
                new RandomSwapFactory()
                {
                    MovesReturned = 50,
                    RampUpSpeed = 4.0,
                },
            };
            localSearch.InitialSolvers.Add(compundSolver);
            return localSearch;
        }
    }
}
