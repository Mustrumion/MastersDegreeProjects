using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            RealInstanceDataLoader loader = new RealInstanceDataLoader();
            loader.Filepath = @"C:\Users\bartl\source\repos\MastersDegreeProjects\FileReaderTests\Resources\hour_3channels.txt";
            Instance instance = loader.LoadInstanceFile();
            Console.WriteLine("End.");
            Console.ReadKey();
        }
    }
}
