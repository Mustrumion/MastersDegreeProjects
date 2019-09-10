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
    public class SimulatedAnnealingTuning2
    {
        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver = new BulkSolver()
            {
                Times = 3,
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                ReportProgrssToFile = true,
                MaxThreads = 15,
                LengthFilter = new[] { "week.json" },
                //KindFilter = new[] { "1_ch_n", "2_ch_n", "2_ch_v", "3_ch_n", "3edu1", "3edu2", "4ch3n1a1", "4ch3n1a2" },
                DifficultyFilter = new[] { "medium" },
                StartingSolutionsDirectory = Path.Combine(MAIN_DIRECTORY, "solutions", "base_solutions_all"),
                SavedSubpath = "annealing_optimization2"
            };
            for(double i = 5; i < 10; i += 0.4)
            {
                for (double j = 5; j < 14; j += 0.9)
                {
                    Console.WriteLine($"started {i} {j}");
                    double alpha = 1.0 - Math.Pow(0.25, i);
                    double beta = Math.Pow(0.25, j);
                    bulkSolver.SolveEverything(() => { return SimulatedAnnealingMakerParametrized(alpha, beta, $"{alpha}_{beta}"); });
                }
            }
        }

       
        private ISolver SimulatedAnnealingMakerParametrized(double alpha, double beta, string name)
        {
            SimulatedAnnealing solver = new SimulatedAnnealing()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(0, 4, 0),
                TemperatureMultiplier = alpha,
                TemperatureAddition = beta,
                StepsAnalyzedWithoutImprovementToStop = 300,
                Description = name,
            };
            return solver;
        }
    }
}
