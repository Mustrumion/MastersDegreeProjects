using ExampleFileReader.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            InstanceDataLoader loader = new InstanceDataLoader();
            loader.Filepath = @"D:\praca inżynierska\generator_output.txt";
            Instance instance = loader.LoadInstanceFile();
            Console.WriteLine("End.");
            Console.ReadKey();
        }
    }
}
