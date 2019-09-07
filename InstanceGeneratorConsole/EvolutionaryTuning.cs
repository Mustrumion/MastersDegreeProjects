using InstanceSolvers;
using InstanceSolvers.Solvers;
using InstanceSolvers.Solvers.Base;
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
                MaxThreads = 1,
                ReportProgrssToFile = true,
                LengthFilter = new[] { "week.json" },
                KindFilter = new[] { "1_ch_n", "2_ch_n", "2_ch_v", "3_ch_n", "3edu1", "3edu2", "4ch3n1a1", "4ch3n1a2" },
                DifficultyFilter = new[] { "medium" },
                StartingSolutionsDirectory = Path.Combine(MAIN_DIRECTORY, "solutions", "base_solutions_all"),
                SavedSubpath = "annealing_optimization"
            };
            Random random = new Random();
            
            while (true)
            {
                int population = random.Next(15, 91);
                int parentCandidates = random.Next(0, 4);
                double mutationRate = random.NextDouble();
                double crosoverRate = random.NextDouble();
                double mutationProbability = random.NextDouble() * 0.01;
                double crossoverProbability = random.NextDouble() * 0.05;
                EvolutionaryMakerParametrized(population, parentCandidates, mutationRate, crosoverRate, mutationProbability, crossoverProbability);
            }
        }

        private ISolver EvolutionaryMakerParametrized(int population, int parentCandidates, double mutationRate, double crosoverRate, double mutationProbability, double crossoverProbability)
        {
            Evolutionary solver = new Evolutionary()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(0, 3, 0),
                PopulationCount = population,
                CandidatesForParent = parentCandidates,
                MutationRate = mutationRate,
                CrossoverRate = crosoverRate,
                NumberOfMutationsToBreakCount = mutationProbability,
                ProportionOfBreaksCrossed = crossoverProbability,
                Description = $"{population}_{parentCandidates}_{mutationRate:0.#####}_{crosoverRate:0.#####}_{mutationProbability:0.#####}_{crossoverProbability:0.#####}",
            };
            return solver;
        }
    }
}
