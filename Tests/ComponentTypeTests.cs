// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public static class ComponentTypeTests
    {
        static ComponentTypeTests()
        {
            // Initialize component types in a predefined order for test consistency.
            ComponentType.TypeOf<Enabled>();
            ComponentType.TypeOf<Name>();
            ComponentType.TypeOf<Position2D>();
            ComponentType.TypeOf<Position3D>();
            ComponentType.TypeOf<Rotation2D>();
            ComponentType.TypeOf<Rotation3D>();
            ComponentType.TypeOf<Scale2D>();
            ComponentType.TypeOf<Scale3D>();
        }

        private static IEnumerable CompareEqualsTestCases
        {
            get
            {
                object?[] parameters = new object[2];

                parameters[0] = null;
                parameters[1] = ComponentType.TypeOf<Enabled>();

                yield return parameters;

                parameters[0] = null;
                parameters[1] = ComponentType.TypeOf<Name>();

                yield return parameters;

                parameters[0] = null;
                parameters[1] = ComponentType.TypeOf<Position2D>();

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Name>();
                parameters[1] = ComponentType.TypeOf<Position2D>();

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Name>();
                parameters[1] = ComponentType.TypeOf<Enabled>();

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Position2D>();
                parameters[1] = ComponentType.TypeOf<Rotation2D>();

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Position2D>();
                parameters[1] = ComponentType.TypeOf<Enabled>();

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Position3D>();
                parameters[1] = ComponentType.TypeOf<Rotation3D>();

                yield return parameters;
            }
        }

        private static IEnumerable TypeOfTestCases
        {
            get
            {
                object[] parameters = new object[5];

                parameters[0] = ComponentType.TypeOf<Enabled>();
                parameters[1] = typeof(Enabled);
                parameters[2] = 0;
                parameters[3] = Unsafe.SizeOf<Enabled>() - 1;
                parameters[4] = ComponentTypeCategory.Tag;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Name>();
                parameters[1] = typeof(Name);
                parameters[2] = 1;
                parameters[3] = Unsafe.SizeOf<Name>();
                parameters[4] = ComponentTypeCategory.Managed;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Position2D>();
                parameters[1] = typeof(Position2D);
                parameters[2] = 2;
                parameters[3] = Unsafe.SizeOf<Position2D>();
                parameters[4] = ComponentTypeCategory.Unmanaged;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Position3D>();
                parameters[1] = typeof(Position3D);
                parameters[2] = 3;
                parameters[3] = Unsafe.SizeOf<Position3D>();
                parameters[4] = ComponentTypeCategory.Unmanaged;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Rotation2D>();
                parameters[1] = typeof(Rotation2D);
                parameters[2] = 4;
                parameters[3] = Unsafe.SizeOf<Rotation2D>();
                parameters[4] = ComponentTypeCategory.Unmanaged;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Rotation3D>();
                parameters[1] = typeof(Rotation3D);
                parameters[2] = 5;
                parameters[3] = Unsafe.SizeOf<Rotation3D>();
                parameters[4] = ComponentTypeCategory.Unmanaged;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Scale2D>();
                parameters[1] = typeof(Scale2D);
                parameters[2] = 6;
                parameters[3] = Unsafe.SizeOf<Scale2D>();
                parameters[4] = ComponentTypeCategory.Unmanaged;

                yield return parameters;

                parameters[0] = ComponentType.TypeOf<Scale3D>();
                parameters[1] = typeof(Scale3D);
                parameters[2] = 7;
                parameters[3] = Unsafe.SizeOf<Scale3D>();
                parameters[4] = ComponentTypeCategory.Unmanaged;

                yield return parameters;
            }
        }

        [Test]
        public static void CompareExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ComponentType.TypeOf<Name>().CompareTo(string.Empty);
            });
        }

        [TestCaseSource(nameof(CompareEqualsTestCases))]
        public static void CompareTest(ComponentType? lesser, ComponentType? greater)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ComponentType.Compare(lesser, lesser), Is.Zero);
                Assert.That(ComponentType.Compare(greater, greater), Is.Zero);
                Assert.That(ComponentType.Compare(lesser, greater), Is.EqualTo(-1));
                Assert.That(ComponentType.Compare(greater, lesser), Is.EqualTo(1));
            }
        }

        [TestCaseSource(nameof(CompareEqualsTestCases))]
        public static void EqualsTest(ComponentType? source, ComponentType? other)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ComponentType.Equals(source, source), Is.True);
                Assert.That(ComponentType.Equals(other, other), Is.True);
                Assert.That(ComponentType.Equals(source, other), Is.False);
                Assert.That(ComponentType.Equals(other, source), Is.False);
            }
        }

        [TestCaseSource(nameof(TypeOfTestCases))]
        public static void TypeOfTest(ComponentType actual, Type expectedType,
            int expectedIdentifier, int expectedSize, ComponentTypeCategory expectedCategory)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Type, Is.EqualTo(expectedType));
                Assert.That(actual.Identifier, Is.EqualTo(expectedIdentifier));
                Assert.That(actual.Size, Is.EqualTo(expectedSize));
                Assert.That(actual.Category, Is.EqualTo(expectedCategory));
            }
        }
    }
}
