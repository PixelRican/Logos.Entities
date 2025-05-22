using System;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityArchetypeTests
    {
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

            expected[0] = actual[4] = ComponentType.TypeOf<Name>();
            expected[1] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Rotation2D>();
            expected[3] = actual[3] = ComponentType.TypeOf<Scale2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Tag>();

            CreateTestHelper(expected, 5, actual, 5);

            expected[0] = actual[0] = ComponentType.TypeOf<Name>();
            expected[1] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[4] = ComponentType.TypeOf<Rotation2D>();
            expected[3] = actual[6] = ComponentType.TypeOf<Scale2D>();
            expected[4] = actual[1] = ComponentType.TypeOf<Position3D>();
            expected[5] = actual[3] = ComponentType.TypeOf<Rotation3D>();
            expected[6] = actual[5] = ComponentType.TypeOf<Scale3D>();
            expected[7] = actual[7] = ComponentType.TypeOf<Tag>();

            CreateTestHelper(expected, 8, actual, 8);
        }

        private static void CreateTestHelper(ComponentType[] expectedComponentTypes,
            int expectedLength, ComponentType[] actualComponentTypes, int actualLength)
        {
            ReadOnlySpan<EntityArchetype> archetypes =
            [
                EntityArchetype.Create(actualComponentTypes),
                EntityArchetype.Create(actualComponentTypes.Take(actualLength)),
                EntityArchetype.Create(actualComponentTypes.AsSpan(0, actualLength))
            ];
            int expectedManagedCount = 0;
            int expectedUnmanagedCount = 0;
            int expectedTagCount = 0;

            for (int i = 0; i < expectedLength; i++)
            {
                switch (expectedComponentTypes[i].Category)
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
                int managedCount = 0;
                int unmanagedCount = 0;
                int tagCount = 0;

                Assert.AreEqual(expectedLength, componentTypes.Length);

                for (int i = 0; i < expectedLength; i++)
                {
                    ComponentType componentType = componentTypes[i];

                    Assert.IsTrue(archetype.Contains(componentType));
                    Assert.AreEqual(expectedComponentTypes[i], componentType);

                    switch (componentType.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            managedCount++;
                            continue;
                        case ComponentTypeCategory.Unmanaged:
                            unmanagedCount++;
                            continue;
                        case ComponentTypeCategory.Tag:
                            tagCount++;
                            continue;
                    }
                }

                Assert.AreEqual(expectedManagedCount, managedCount);
                Assert.AreEqual(expectedUnmanagedCount, unmanagedCount);
                Assert.AreEqual(expectedTagCount, tagCount);
            }

            Array.Clear(expectedComponentTypes, 0, expectedLength);
            Array.Clear(actualComponentTypes, 0, actualLength);
        }
    }
}
