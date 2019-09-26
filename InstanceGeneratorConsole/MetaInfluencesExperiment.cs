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
    public class MetaInfluencesExperiment
    {
        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver2 = new BulkSolver()
            {
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                MaxThreads = 15,
                ReportProgrssToFile = true,
                PickOnlyBestStartingSolutions = true,
                LengthFilter = new[] { "week.json", "month.json" },
                StartingSolutionsDirectory = Path.Combine(MAIN_DIRECTORY, "solutions", "base_solutions_all"),
            };
            bulkSolver2.SolveEverything(SimulatedAnnealingBest);
            BulkSolver bulkSolver = new BulkSolver()
            {
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = false,
                ReportProgrssToFile = true,
                PickOnlyBestStartingSolutions = true,
                LengthFilter = new[] { "week.json", "month.json" },
                StartingSolutionsDirectory = Path.Combine(MAIN_DIRECTORY, "solutions", "base_solutions_all"),
            };
            bulkSolver.SolveEverything(EvolutionaryBest);
        }

        
        private ISolver SimulatedAnnealingBest()
        {
            SimulatedAnnealing solver = new SimulatedAnnealing()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(1, 0, 0),
                TemperatureMultiplier = 0.999973432905173,
                TemperatureAddition = 3.51023731470062E-06,
                StepsAnalyzedWithoutImprovementToStop = 300,
                Description = "annealing_best_tests_all",
            };
            return solver;
        }


        private ISolver EvolutionaryBest()
        {
            Evolutionary solver = new Evolutionary()
            {
                ParallelAllowed = true,
                ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 15 },
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                BreakAfterLoopsWithoutImprovement = 2,
                TimeLimit = new TimeSpan(1, 0, 0),
                PopulationCount = 38,
                CandidatesForParent = 4,
                MutationRate = 0.394736842105263,
                CrossoverRate = 0.657894736842105,
                NumberOfMutationsToBreakCount = 0.000790520968624,
                ProportionOfBreaksCrossed = 0.083086983070447,
                GenerationImproverGenerator = LocalSearchForEvolutionaryImprovement,
                Description = "evolutionary_best_tests_all",
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
