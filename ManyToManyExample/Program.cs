using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.ResizableDirectMemory.Generic;
using Platform.Data.Numbers.Raw;
using Platform.Setters;

namespace ManyToManyExample
{
    class Program
    {
        private static AddressToRawNumberConverter<uint> _addressToRawNumberConverter;
        private static uint _root;
        private static uint _object;
        private static uint _tag;
        private static uint _tagsProperty;

        static void Main(string[] args)
        {
            LinksConstants<uint> constants = new LinksConstants<uint>(enableExternalReferencesSupport: true);

            using (var memory = new Platform.Memory.FileMappedResizableDirectMemory("db.links"))
            using (var disposableLinks = new ResizableDirectMemoryLinks<uint>(memory, ResizableDirectMemoryLinks<uint>.DefaultLinksSizeStep, constants, useAvlBasedIndex: false))
            {
                var links = disposableLinks.DecorateWithAutomaticUniquenessAndUsagesResolution();
                links = new LinksItselfConstantToSelfReferenceResolver<uint>(links);

                _addressToRawNumberConverter = new AddressToRawNumberConverter<uint>();

                InitMarkers(links);
                GenerateData(links);
                QueryFromTagsByObjects(links);
                QueryFromObjecsByTags(links);
            }
        }

        private static void InitMarkers(ILinks<uint> links)
        {
            // Markers
            //  root
            //  object
            //  tag
            //  tagsProperty
            _root = links.GetOrCreate(1U, 1U);
            _object = links.GetOrCreate(_root, _addressToRawNumberConverter.Convert(1U));
            _tag = links.GetOrCreate(_root, _addressToRawNumberConverter.Convert(2U));
            _tagsProperty = links.GetOrCreate(_root, _addressToRawNumberConverter.Convert(3U));
        }

        private static void GenerateData(ILinks<uint> links)
        {
            // Objects
            //  (object1: object1 object)
            // Tags
            //  (tag1: tag1 tag)
            // Objects to Tags
            //  ((object1 tagsProperty) tag1)
            //  ((object1 tagsProperty) tag2)
            //  ((object2 tagsProperty) tag1)
            //  ((object2 tagsProperty) tag2)

            var any = links.Constants.Any;
            if (links.Count(any, any, _object) == 0 && links.Count(any, any, _tag) == 0)
            {
                // No data yet
                var itself = links.Constants.Itself;
                // Generating Objects
                List<uint> objects = new List<uint>();
                for (var i = 0; i < 10; i++)
                {
                    objects.Add(links.CreateAndUpdate(itself, _object));
                }
                // Generating Tags
                List<uint> tags = new List<uint>();
                for (var i = 0; i < 10; i++)
                {
                    tags.Add(links.CreateAndUpdate(itself, _tag));
                }
                // Generation Objects to Tags relationships
                var random = new Random();
                for (var i = 0; i < 100; i++)
                {
                    var @object = objects[random.Next(0, objects.Count)];
                    var tag = tags[random.Next(0, tags.Count)];
                    links.GetOrCreate(links.GetOrCreate(@object, _tagsProperty), tag);
                }
            }
        }

        private static void QueryFromTagsByObjects(ILinks<uint> links)
        {
            uint @object = GetObject(links);
            List<uint> tags = GetTagsByObject(links, @object);
            Console.WriteLine("Tags: ");
            for (int i = 0; i < tags.Count; i++)
            {
                Console.WriteLine(tags[i]);
            }
        }

        private static List<uint> GetTagsByObject(ILinks<uint> links, uint @object)
        {
            List<uint> tags = new List<uint>();
            var property = links.SearchOrDefault(@object, _tagsProperty);
            if (property != links.Constants.Null)
            {
                var any = links.Constants.Any;
                var @continue = links.Constants.Continue;
                var target = links.Constants.TargetPart;
                var query = new Link<uint>(any, property, any);
                links.Each(link =>
                {
                    tags.Add(link[target]);
                    return @continue;
                }, query);
            }
            return tags;
        }

        private static uint GetObject(ILinks<uint> links)
        {
            return GetFirstLinkWithMarker(links, _object);
        }

        private static void QueryFromObjecsByTags(ILinks<uint> links)
        {
            uint tag = GetTag(links);
            List<uint> objects = GetObjectsByTag(links, tag);
            Console.WriteLine("Objects: ");
            for (int i = 0; i < objects.Count; i++)
            {
                Console.WriteLine(objects[i]);
            }
        }

        private static List<uint> GetObjectsByTag(ILinks<uint> links, uint tag)
        {
            HashSet<uint> objects = new HashSet<uint>();
            var any = links.Constants.Any;
            var @continue = links.Constants.Continue;
            var source = links.Constants.SourcePart;
            var query = new Link<uint>(any, any, tag);
            links.Each(link =>
            {
                var @object = links.GetLink(link[source])[source];
                objects.Add(@object);
                return @continue;
            }, query);
            return objects.ToList();
        }

        private static uint GetTag(ILinks<uint> links)
        {
            return GetFirstLinkWithMarker(links, _tag);
        }

        private static uint GetFirstLinkWithMarker(ILinks<uint> links, uint marker)
        {
            var setter = new Setter<uint, uint>(links.Constants.Continue, links.Constants.Break, links.Constants.Null);
            var any = links.Constants.Any;
            links.Each(setter.SetFirstAndReturnFalse, new Link<uint>(any, any, marker));
            return setter.Result;
        }
    }
}
