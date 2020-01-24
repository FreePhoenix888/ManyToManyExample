using BenchmarkDotNet.Running;
using System;

namespace AdvancedExample
{
    class Program
    {
        //static void Main() => BenchmarkRunner.Run<AdvancedExampleBenchmark>();

        static void Main()
        {
            var storage = new ObjectsAndTagsStorage();
            storage.InitMarkers();
            storage.GenerateData(100000000, 10000, 10);
            var tags = new uint[] { storage.GetTag(), storage.GetTag(), storage.GetTag(), storage.GetTag(), storage.GetTag() };
            Console.WriteLine("Tags: ");
            for (int i = 0; i < tags.Length; i++)
            {
                Console.WriteLine(tags[i]);
            }
            var objects = storage.GetObjectsByTags(tags);
            Console.WriteLine("Objects: ");
            for (int i = 0; i < objects.Count; i++)
            {
                Console.WriteLine(objects[i]);
            }
        }
    }
}
