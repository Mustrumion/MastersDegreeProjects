using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceSolvers;
using InstanceSolvers.MoveFactories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = @"C:\Users\Mustrum\Desktop\dane\hour_DS_D_DH_inst.json";
            var reader = new InstanceJsonSerializer
            {
                Path = file
            };
            Instance instance = reader.DeserializeInstance();
            LocalRandomSearch solver = new LocalRandomSearch()
            {
                Instance = instance,
                Seed = 10,
                ScoringFunction = new Scorer(),
            };
            solver.MoveFactories = new List<IMoveFactory>
            {
                new InsertMoveFactory(solver.Solution)
                {
                    IgnoreWhenUnitOverfillAbove = 20,
                    Random = solver.Random,
                }
            };
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\hour_DS_D_DH_sol_localsteep.json"
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
        }
    }
}
