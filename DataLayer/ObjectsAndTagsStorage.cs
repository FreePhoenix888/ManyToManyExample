using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Disposables;
using Platform.Memory;
using Platform.Ranges;
using Platform.Setters;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.ResizableDirectMemory.Generic;
using Platform.Data.Numbers.Raw;

namespace DataLayer
{
    public class ObjectsAndTagsStorage : DisposableBase
    {
        private readonly ILinks<uint> _links;
        private readonly FileMappedResizableDirectMemory _memory;
        private readonly ResizableDirectMemoryLinks<uint> _memoryManager;
        private readonly AddressToRawNumberConverter<uint> _addressToRawNumberConverter;
        private uint _root;
        private uint _object;
        private uint _tag;
        private Range<uint> _objectsRange;
        private Range<uint> _tagsRange;

        public ObjectsAndTagsStorage(string path, long minimumStorageSizeInBytes)
        {
            var constants = new LinksConstants<uint>(enableExternalReferencesSupport: true);
            _memory = new FileMappedResizableDirectMemory(path);
            _memoryManager = new ResizableDirectMemoryLinks<uint>(_memory, minimumStorageSizeInBytes, constants, useAvlBasedIndex: false);

            _links = _memoryManager.DecorateWithAutomaticUniquenessAndUsagesResolution();
            _links = new LinksItselfConstantToSelfReferenceResolver<uint>(_links);

            _addressToRawNumberConverter = new AddressToRawNumberConverter<uint>();
        }

        public void InitMarkers()
        {
            // Markers
            //  root
            //  object
            //  tag
            _root = _links.GetOrCreate(1U, 1U);
            _object = _links.GetOrCreate(_root, _addressToRawNumberConverter.Convert(1U));
            _tag = _links.GetOrCreate(_root, _addressToRawNumberConverter.Convert(2U));
        }

        public void GenerateData(long maximumObjects, long maximumTags, long maximumTagsPerObject)
        {
            // Objects
            //  (object1: object1 object)
            // Tags
            //  (tag1: tag1 tag)
            // Objects to Tags
            //  (object1 tag1)
            //  (object1 tag2)
            //  (object2 tag1)
            //  (object2 tag2)

            var any = _links.Constants.Any;
            if (_links.Count(any, any, _object) == 0 && _links.Count(any, any, _tag) == 0)
            {
                // No data yet
                var itself = _links.Constants.Itself;
                // Generating Objects
                var objectsRangeStart = _links.Count() + 1;
                for (var i = 0L; i < maximumObjects; i++)
                {
                    _links.CreateAndUpdate(itself, _object);
                }
                var objectsRangeEnd = _links.Count() + 1;
                _objectsRange = new Range<uint>(objectsRangeStart, objectsRangeEnd);
                // Generating Tags
                var tagsRangeStart = _links.Count() + 1;
                for (var i = 0L; i < maximumTags; i++)
                {
                    _links.CreateAndUpdate(itself, _tag);
                }
                var tagsRangeEnd = _links.Count() + 1;
                _tagsRange = new Range<uint>(tagsRangeStart, tagsRangeEnd);
                // Generation Objects to Tags relationships
                var random = new Random();
                for (var i = 0L; i < maximumObjects; i++)
                {
                    var @object = (uint)(objectsRangeStart + i);
                    for (var j = 0L; j < maximumTagsPerObject; j++)
                    {
                        var tag = (uint)random.Next((int)tagsRangeStart, (int)tagsRangeEnd);
                        _links.GetOrCreate(@object, tag);
                    }
                }
            }
            else
            {
                var cursor = 4U;
                _objectsRange = new Range<uint>(cursor, cursor += (uint)maximumObjects);
                _tagsRange = new Range<uint>(cursor, cursor += (uint)maximumTags);
            }
        }

        public void QueryFromTagsByObjects()
        {
            var @object = GetObject();
            var tags = GetTagsByObject(@object);
            Console.WriteLine("Tags: ");
            for (int i = 0; i < tags.Count; i++)
            {
                Console.WriteLine(tags[i]);
            }
        }

        private List<uint> GetTagsByObject(uint @object)
        {
            var tags = new List<uint>();
            var any = _links.Constants.Any;
            var @continue = _links.Constants.Continue;
            var target = _links.Constants.TargetPart;
            var query = new Link<uint>(any, @object, any);
            _links.Each(link =>
            {
                // Ignore objects itself
                if (Point<uint>.IsPartialPoint(link))
                {
                    return @continue;
                }
                tags.Add(link[target]);
                return @continue;
            }, query);
            return tags;
        }

        private uint GetObject()
        {
            //return GetFirstLinkWithMarker(_object);
            return GetRandomObject();
        }

        private uint GetRandomObject()
        {
            Random random = new Random();
            return (uint)random.Next((int)_objectsRange.Minimum, (int)_objectsRange.Maximum);
        }

        public void QueryFromObjecsByTags()
        {
            var tag = GetTag();
            var objects = GetObjectsByTag(tag);
            Console.WriteLine("Objects: ");
            for (int i = 0; i < objects.Count; i++)
            {
                Console.WriteLine(objects[i]);
            }
        }

        public List<uint> GetObjectsByAllTags(params uint[] tags)
        {
            var objects = new List<uint>();
            if (tags.Length > 1)
            {
                AddObjectsByAllTag(objects, tags);
            }
            else if (tags.Length == 1)
            {
                AddObjectsByTag(objects, tags[0]);
            }
            return objects;
        }

        private void AddObjectsByAllTag(List<uint> objects, params uint[] tags)
        {
            var any = _links.Constants.Any;
            var @continue = _links.Constants.Continue;
            var source = _links.Constants.SourcePart;
            var lengthMinusOne = tags.Length - 1;
            var lastTag = tags[lengthMinusOne];
            var tagsExceptLastOne = new ArraySegment<uint>(tags, 0, lengthMinusOne);
            var query = new Link<uint>(any, any, lastTag);
            _links.Each(link =>
            {
                var @object = link[source];
                if (ObjectContainsAllTags(@object, tagsExceptLastOne))
                {
                    objects.Add(@object);
                }
                return @continue;
            }, query);
        }

        private bool ObjectContainsAllTags(uint @object, IList<uint> tags)
        {
            for (var i = 0; i < tags.Count; i++)
            {
                if (!_links.Exists(@object, tags[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public List<uint> GetObjectsByAnyTags(params uint[] tags)
        {
            var objects = new HashSet<uint>();
            for (var i = 0; i < tags.Length; i++)
            {
                AddObjectsByTag(objects, tags[i]);
            }
            return objects.ToList();
        }

        private void AddObjectsByTag(HashSet<uint> objects, uint tag)
        {
            var any = _links.Constants.Any;
            var @continue = _links.Constants.Continue;
            var source = _links.Constants.SourcePart;
            var query = new Link<uint>(any, any, tag);
            _links.Each(link =>
            {
                objects.Add(link[source]);
                return @continue;
            }, query);
        }

        private List<uint> GetObjectsByTag(uint tag)
        {
            var objects = new List<uint>();
            AddObjectsByTag(objects, tag);
            return objects;
        }

        private void AddObjectsByTag(List<uint> objects, uint tag)
        {
            var any = _links.Constants.Any;
            var @continue = _links.Constants.Continue;
            var source = _links.Constants.SourcePart;
            var query = new Link<uint>(any, any, tag);
            _links.Each(link =>
            {
                objects.Add(link[source]);
                return @continue;
            }, query);
        }

        public uint GetTag()
        {
            //return GetFirstLinkWithMarker(_tag);
            return GetRandomTag();
        }

        private uint GetRandomTag()
        {
            Random random = new Random();
            return (uint)random.Next((int)_tagsRange.Minimum, (int)_tagsRange.Maximum);
        }

        private uint GetFirstLinkWithMarker(uint marker)
        {
            var setter = new Setter<uint, uint>(_links.Constants.Continue, _links.Constants.Break, _links.Constants.Null);
            var any = _links.Constants.Any;
            _links.Each(setter.SetFirstAndReturnFalse, new Link<uint>(any, any, marker));
            return setter.Result;
        }

        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                _memoryManager.DisposeIfNotDisposed();
                _memory.DisposeIfNotDisposed();
            }
        }
    }
}
