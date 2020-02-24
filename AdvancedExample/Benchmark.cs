using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DataLayer;

namespace AdvancedExample
{
    public class AdvancedExampleBenchmark
    {
        private readonly ObjectsAndTagsStorage _storage;
        private readonly uint[] _tags2;
        private readonly uint[] _tags5;

        public AdvancedExampleBenchmark()
        {
            _storage = new ObjectsAndTagsStorage(@"F:\db.links", 48L * 1024L * 1024L * 1024L);
            _storage.InitMarkers();
            _storage.GenerateData(100000000, 10000, 10);
            _tags2 = new uint[] { _storage.GetTag(), _storage.GetTag() };
            _tags5 = new uint[] { _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag() };
        }

        [Benchmark]
        public List<uint> GetObjectsByAll2Tags()
        {
            List<uint> result = null;
            for (int i = 0; i < 10; i++)
            {
                result = _storage.GetObjectsByAllTags(_tags2);
            }
            return result;
        }

        [Benchmark]
        public List<uint> GetObjectsByAll5Tags()
        {
            List<uint> result = null;
            for (int i = 0; i < 10; i++)
            {
                result = _storage.GetObjectsByAllTags(_tags5);
            }
            return result;
        }
    }
}
