using System;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class ComponentTypeTests
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            ComponentType.TypeOf<Position2D>();
            ComponentType.TypeOf<Rotation2D>();
            ComponentType.TypeOf<Scale2D>();
            ComponentType.TypeOf<Position3D>();
            ComponentType.TypeOf<Rotation3D>();
            ComponentType.TypeOf<Scale3D>();
            ComponentType.TypeOf<Name>();
            ComponentType.TypeOf<Tag>();
        }

        [TestMethod]
        public void CompareTest()
        {
            ReadOnlySpan<ComponentType> expectedComponentTypes =
            [
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>()
            ];
            Span<ComponentType> actualComponentTypes =
            [
                ComponentType.TypeOf<Tag>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Scale2D>()
            ];

            actualComponentTypes.Sort();

            for (int i = 0; i < 8; i++)
            {
                ComponentType actualComponentType = actualComponentTypes[i];

                Assert.AreEqual(0, actualComponentType.CompareTo(expectedComponentTypes[i]));
                Assert.AreEqual(1, actualComponentType.CompareTo(null));
                Assert.ThrowsException<ArgumentException>(() => actualComponentType.CompareTo(this));
            }
        }

        [TestMethod]
        public void EqualsTest()
        {
            ReadOnlySpan<ComponentType> componentTypes =
            [
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Tag>()
            ];

            ComponentType previous = null!;

            foreach (ComponentType current in componentTypes)
            {
                Assert.AreEqual(current, current);
                Assert.AreNotEqual(previous, current);

                previous = current;
            }
        }

        [TestMethod]
        public void TypeOfTest()
        {
            TypeOfTestHelper<Position2D>(0);
            TypeOfTestHelper<Rotation2D>(1);
            TypeOfTestHelper<Scale2D>(2);
            TypeOfTestHelper<Position3D>(3);
            TypeOfTestHelper<Rotation3D>(4);
            TypeOfTestHelper<Scale3D>(5);
            TypeOfTestHelper<Name>(6);
            TypeOfTestHelper<Tag>(7);
        }

        private static void TypeOfTestHelper<T>(int expectedId)
        {
            Assert.AreEqual(expectedId, ComponentType.TypeOf<T>().Id);
            Assert.AreEqual(typeof(T), ComponentType.TypeOf<T>().Type);

            switch (ComponentType.TypeOf<T>().Category)
            {
                case ComponentTypeCategory.Managed:
                    Assert.IsTrue(RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                    Assert.AreEqual(Unsafe.SizeOf<T>(), ComponentType.TypeOf<T>().Size);
                    return;
                case ComponentTypeCategory.Unmanaged:
                    Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                    Assert.AreEqual(Unsafe.SizeOf<T>(), ComponentType.TypeOf<T>().Size);
                    return;
                case ComponentTypeCategory.Tag:
                    Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                    Assert.AreEqual(Unsafe.SizeOf<T>() - 1, ComponentType.TypeOf<T>().Size);
                    return;
                default:
                    Assert.Fail("Invalid ComponentTypeCategory found.");
                    return;
            }
        }
    }
}
