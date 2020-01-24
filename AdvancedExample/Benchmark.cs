using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace AdvancedExample
{
    public class AdvancedExampleBenchmark
    {
        private readonly ObjectsAndTagsStorage _storage;
        private readonly uint[] _tags2;
        private readonly uint[] _tags5;

        public AdvancedExampleBenchmark()
        {
            _storage = new ObjectsAndTagsStorage(@"F:\DataArchive\db.links");
            _storage.InitMarkers();
            _storage.GenerateData(100000000, 10000, 10);
            _tags2 = new uint[] { _storage.GetTag(), _storage.GetTag() };
            _tags5 = new uint[] { _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag(), _storage.GetTag() };
        }

        [Benchmark]
        public List<uint> GetObjectsBy2Tags()
        {
            List<uint> result = null;
            for (int i = 0; i < 10; i++)
            {
                result = _storage.GetObjectsByAnyTags(_tags2);
            }
            return result;
        }

        [Benchmark]
        public List<uint> GetObjectsBy5Tags()
        {
            List<uint> result = null;
            for (int i = 0; i < 10; i++)
            {
                result = _storage.GetObjectsByAnyTags(_tags5);
            }
            return result;
        }
    }
}
