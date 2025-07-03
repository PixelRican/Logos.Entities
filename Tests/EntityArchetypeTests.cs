// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityArchetypeTests
    {
        [Test]
        public void AddTest()
        {
            Span<ComponentType> types =
            [
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Enabled>(),
            ];

            AddTestHelper(types);

            types.Reverse();
            AddTestHelper(types);

            Random.Shared.Shuffle(types);
            AddTestHelper(types);
        }

        private static void AddTestHelper(ReadOnlySpan<ComponentType> types)
        {
            EntityArchetype subarchetype = EntityArchetype.Base;

            foreach (ComponentType type in types)
            {
                EntityArchetype superarchetype = subarchetype.Add(type);
                AddRemoveAssertHelper(subarchetype, superarchetype, type);
                subarchetype = superarchetype;
            }
        }

        private static void AddRemoveAssertHelper(EntityArchetype subarchetype,
            EntityArchetype superarchetype, ComponentType type)
        {
            ReadOnlySpan<ComponentType> subset = subarchetype.ComponentTypes;
            ReadOnlySpan<ComponentType> superset = superarchetype.ComponentTypes;
            int index = ~subset.BinarySearch(type);

            Assert.That(index, Is.EqualTo(superset.BinarySearch(type)));
            Assert.That(subarchetype.Contains(type), Is.False);
            Assert.That(superarchetype.Contains(type));
            Assert.That(subset.Slice(0, index).SequenceEqual(superset.Slice(0, index)));
            Assert.That(subset.Slice(index).SequenceEqual(superset.Slice(index + 1)));
            Assert.That(subarchetype, Is.SameAs(subarchetype.Add(null!)));
            Assert.That(superarchetype, Is.SameAs(superarchetype.Add(type)));
        }

        [Test]
        public void CreateTest()
        {
            ComponentType[] expected = new ComponentType[8];
            ComponentType[] actual = new ComponentType[8];

            CreateTestHelper(expected, 0, actual, 0);

            expected[0] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[1] = actual[0] = ComponentType.TypeOf<Rotation2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Scale2D>();

            CreateTestHelper(expected, 3, actual, 3);

            expected[0] = actual[2] = actual[3] = ComponentType.TypeOf<Position3D>();
            expected[1] = actual[1] = actual[4] = ComponentType.TypeOf<Rotation3D>();
            expected[2] = actual[0] = actual[5] = ComponentType.TypeOf<Scale3D>();

            CreateTestHelper(expected, 3, actual,6);

            expected[0] = actual[4] = ComponentType.TypeOf<Name>();
            expected[1] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Rotation2D>();
            expected[3] = actual[3] = ComponentType.TypeOf<Scale2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Enabled>();

            CreateTestHelper(expected, 5, actual, 5);

            expected[0] = actual[1] = ComponentType.TypeOf<Name>();
            expected[1] = actual[3] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[5] = ComponentType.TypeOf<Position3D>();
            expected[3] = actual[7] = ComponentType.TypeOf<Rotation2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Rotation3D>();
            expected[5] = actual[2] = ComponentType.TypeOf<Scale2D>();
            expected[6] = actual[4] = ComponentType.TypeOf<Scale3D>();
            expected[7] = actual[6] = ComponentType.TypeOf<Enabled>();

            CreateTestHelper(expected, 8, actual, 8);
        }

        private static void CreateTestHelper(ComponentType[] expectedArray,
            int expectedLength, ComponentType[] actualArray, int actualLength)
        {
            ReadOnlySpan<EntityArchetype> archetypes =
            [
                EntityArchetype.Create(actualArray),
                EntityArchetype.Create(actualArray.Take(actualLength)),
                EntityArchetype.Create(actualArray.AsSpan(0, actualLength))
            ];
            int expectedManagedCount = 0;
            int expectedUnmanagedCount = 0;
            int expectedTagCount = 0;

            for (int i = 0; i < expectedLength; i++)
            {
                switch (expectedArray[i].Category)
                {
                    case ComponentTypeCategory.Managed:
                        expectedManagedCount++;
                        continue;
                    case ComponentTypeCategory.Unmanaged:
                        expectedUnmanagedCount++;
                        continue;
                    case ComponentTypeCategory.Tag:
                        expectedTagCount++;
                        continue;
                }
            }

            foreach (EntityArchetype archetype in archetypes)
            {
                ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes;
                int actualManagedCount = 0;
                int actualUnmanagedCount = 0;
                int actualTagCount = 0;

                Assert.That(expectedLength, Is.EqualTo(componentTypes.Length));

                for (int i = 0; i < expectedLength; i++)
                {
                    ComponentType componentType = expectedArray[i];

                    Assert.That(archetype.Contains(componentType));
                    Assert.That(componentType, Is.EqualTo(componentTypes[i]));

                    switch (componentType.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            actualManagedCount++;
                            continue;
                        case ComponentTypeCategory.Unmanaged:
                            actualUnmanagedCount++;
                            continue;
                        case ComponentTypeCategory.Tag:
                            actualTagCount++;
                            continue;
                    }
                }

                Assert.That(expectedManagedCount, Is.EqualTo(actualManagedCount));
                Assert.That(expectedUnmanagedCount, Is.EqualTo(actualUnmanagedCount));
                Assert.That(expectedTagCount, Is.EqualTo(actualTagCount));
            }

            Array.Clear(expectedArray, 0, expectedLength);
            Array.Clear(actualArray, 0, actualLength);
        }

        [Test]
        public void EquatableTest()
        {
            ReadOnlySpan<EntityArchetype> span =
            [
                EntityArchetype.Base,
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Name>(),
                                        ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Name>(),
                                        ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Enabled>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Name>(),
                                        ComponentType.TypeOf<Position3D>(),
                                        ComponentType.TypeOf<Rotation3D>(),
                                        ComponentType.TypeOf<Scale3D>(),
                                        ComponentType.TypeOf<Enabled>()])
            ];
            EntityArchetype previous = null!;

            foreach (EntityArchetype current in span)
            {
                Assert.That(current, Is.EqualTo(current));
                Assert.That(current, Is.EqualTo(EntityArchetype.Create(current.ComponentTypes)));
                Assert.That(previous, Is.Not.EqualTo(current));

                previous = current;
            }
        }

        [Test]
        public void RemoveTest()
        {
            Span<ComponentType> types =
            [
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Enabled>(),
            ];

            RemoveTestHelper(types);

            types.Reverse();
            RemoveTestHelper(types);

            Random.Shared.Shuffle(types);
            RemoveTestHelper(types);
        }

        private static void RemoveTestHelper(ReadOnlySpan<ComponentType> types)
        {
            EntityArchetype superarchetype = EntityArchetype.Create(types);

            foreach (ComponentType type in types)
            {
                EntityArchetype subarchetype = superarchetype.Remove(type);
                AddRemoveAssertHelper(subarchetype, superarchetype, type);
                superarchetype = subarchetype;
            }
        }
    }
}
