using BenchmarkDotNet.Running;

namespace AdvancedExample
{
    class Program
    {
        //static void Main() => BenchmarkRunner.Run<AdvancedExampleBenchmark>();

        static void Main()
        {
            var _storage = new ObjectsAndTagsStorage();
            _storage.InitMarkers();
            _storage.GenerateData(100000000, 10000, 10);
            var _tags = new uint[] { _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag() };


        }
    }
}
