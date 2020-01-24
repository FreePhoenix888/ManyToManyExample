using Platform.Data.Doublets.ResizableDirectMemory.Generic;
using DataLayer;

namespace SimpleExample
{
    class Program
    {
        static void Main()
        {
            var storage = new ObjectsAndTagsStorage(@"db.links", ResizableDirectMemoryLinks<uint>.DefaultLinksSizeStep);
            storage.InitMarkers();
            storage.GenerateData(10, 10, 10);
            storage.QueryFromTagsByObjects();
            storage.QueryFromObjecsByTags();
        }
    }
}
