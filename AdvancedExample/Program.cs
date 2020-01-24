using System;
using BenchmarkDotNet.Running;
using DataLayer;

namespace AdvancedExample
{
    class Program
    {
        static void Main() => BenchmarkRunner.Run<AdvancedExampleBenchmark>();

        //static void Main()
        //{
        //    var storage = new ObjectsAndTagsStorage(@"F:\DataArchive\db.links", 48L * 1024L * 1024L * 1024L);
        //    storage.InitMarkers();
        //    storage.GenerateData(100000000, 10000, 10);
        //    var tags = new uint[] { storage.GetTag(), storage.GetTag() };
        //    //var tags = new uint[] { 100002764, 100001285 };
        //    Console.WriteLine("Tags: ");
        //    for (int i = 0; i < tags.Length; i++)
        //    {
        //        Console.WriteLine(tags[i]);
        //    }
        //    var objects = storage.GetObjectsByAllTags(tags);
        //    Console.WriteLine("Objects: ");
        //    for (int i = 0; i < objects.Count; i++)
        //    {
        //        Console.WriteLine(objects[i]);
        //    }
        //}
    }
}
