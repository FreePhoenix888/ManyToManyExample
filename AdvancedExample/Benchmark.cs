using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace AdvancedExample
{
    public class AdvancedExampleBenchmark
    {
        private readonly ObjectsAndTagsStorage _storage;
        private readonly uint[] _tags;

        public AdvancedExampleBenchmark()
        {
            _storage = new ObjectsAndTagsStorage();
            _storage.InitMarkers();
            _storage.GenerateData(100000000, 10000, 10);
            _tags = new uint[] { _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag() };
        }

        [Benchmark]
        public List<uint> GetObjectsByTags() => _storage.GetObjectsByTags(_tags);
    }
}
