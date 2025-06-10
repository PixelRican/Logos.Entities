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
            ComponentType.TypeOf<Position3D>();
            ComponentType.TypeOf<Rotation2D>();
            ComponentType.TypeOf<Rotation3D>();
            ComponentType.TypeOf<Scale2D>();
            ComponentType.TypeOf<Scale3D>();
            ComponentType.TypeOf<Tag>();
            ComponentType.TypeOf<User>();
        }

        [TestMethod]
        public void ComparableTest()
        {
            ReadOnlySpan<ComponentType> expectedSpan =
            [
                ComponentType.TypeOf<User>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>()
            ];
            Span<ComponentType> actualSpan =
            [
                ComponentType.TypeOf<Tag>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<User>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale3D>()
            ];

            actualSpan.Sort();

            for (int i = 0; i < 8; i++)
            {
                ComponentType actual = actualSpan[i];

                Assert.AreEqual(0, actual.CompareTo(expectedSpan[i]));
                Assert.AreEqual(1, actual.CompareTo(null));
                Assert.ThrowsException<ArgumentException>(() => actual.CompareTo(this));
            }
        }

        [TestMethod]
        public void EquatableTest()
        {
            ReadOnlySpan<ComponentType> span =
            [
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>(),
                ComponentType.TypeOf<User>()
            ];
            ComponentType previous = null!;

            foreach (ComponentType current in span)
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
            TypeOfTestHelper<Position3D>(1);
            TypeOfTestHelper<Rotation2D>(2);
            TypeOfTestHelper<Rotation3D>(3);
            TypeOfTestHelper<Scale2D>(4);
            TypeOfTestHelper<Scale3D>(5);
            TypeOfTestHelper<Tag>(6);
            TypeOfTestHelper<User>(7);
        }

        private static void TypeOfTestHelper<T>(int expectedId)
        {
            Assert.AreEqual(expectedId, ComponentType.TypeOf<T>().ID);
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
