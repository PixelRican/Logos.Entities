using System;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityArchetypeTests
    {
        [TestMethod]
        public void AddTest()
        {
            Span<ComponentType> types =
            [
                ComponentType.TypeOf<User>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>(),
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

            Assert.AreEqual(index, superset.BinarySearch(type));
            Assert.IsFalse(subarchetype.Contains(type));
            Assert.IsTrue(superarchetype.Contains(type));
            Assert.IsTrue(subset.Slice(0, index).SequenceEqual(superset.Slice(0, index)));
            Assert.IsTrue(subset.Slice(index).SequenceEqual(superset.Slice(index + 1)));
            Assert.AreSame(subarchetype, subarchetype.Add(null!));
            Assert.AreSame(superarchetype, superarchetype.Add(type));
        }

        [TestMethod]
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

            expected[0] = actual[4] = ComponentType.TypeOf<User>();
            expected[1] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Rotation2D>();
            expected[3] = actual[3] = ComponentType.TypeOf<Scale2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Tag>();

            CreateTestHelper(expected, 5, actual, 5);

            expected[0] = actual[1] = ComponentType.TypeOf<User>();
            expected[1] = actual[3] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[5] = ComponentType.TypeOf<Position3D>();
            expected[3] = actual[7] = ComponentType.TypeOf<Rotation2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Rotation3D>();
            expected[5] = actual[2] = ComponentType.TypeOf<Scale2D>();
            expected[6] = actual[4] = ComponentType.TypeOf<Scale3D>();
            expected[7] = actual[6] = ComponentType.TypeOf<Tag>();

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

                Assert.AreEqual(expectedLength, componentTypes.Length);

                for (int i = 0; i < expectedLength; i++)
                {
                    ComponentType componentType = expectedArray[i];

                    Assert.IsTrue(archetype.Contains(componentType));
                    Assert.AreEqual(componentType, componentTypes[i]);

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

                Assert.AreEqual(expectedManagedCount, actualManagedCount);
                Assert.AreEqual(expectedUnmanagedCount, actualUnmanagedCount);
                Assert.AreEqual(expectedTagCount, actualTagCount);
            }

            Array.Clear(expectedArray, 0, expectedLength);
            Array.Clear(actualArray, 0, actualLength);
        }

        [TestMethod]
        public void EquatableTest()
        {
            ReadOnlySpan<EntityArchetype> span =
            [
                EntityArchetype.Base,
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<User>(),
                                        ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<User>(),
                                        ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Tag>()]),
                EntityArchetype.Create([ComponentType.TypeOf<User>(),
                                        ComponentType.TypeOf<Position3D>(),
                                        ComponentType.TypeOf<Rotation3D>(),
                                        ComponentType.TypeOf<Scale3D>(),
                                        ComponentType.TypeOf<Tag>()])
            ];
            EntityArchetype previous = null!;

            foreach (EntityArchetype current in span)
            {
                Assert.AreEqual(current, current);
                Assert.AreEqual(current, EntityArchetype.Create(current.ComponentTypes));
                Assert.AreNotEqual(previous, current);

                previous = current;
            }
        }

        [TestMethod]
        public void RemoveTest()
        {
            Span<ComponentType> types =
            [
                ComponentType.TypeOf<User>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>(),
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
