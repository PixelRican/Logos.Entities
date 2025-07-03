// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;

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
                yield return new object[]
                {
                    null!, ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    null!, ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    null!, ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Rotation2D>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position3D>(), ComponentType.TypeOf<Rotation3D>()
                };
            }
        }

        private static IEnumerable TypeOfTestCases
        {
            get
            {
                yield return new object[]
                {
                    ComponentType.TypeOf<Enabled>(), typeof(Enabled), 0, 0, ComponentTypeCategory.Tag
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(), typeof(Name), 1, nint.Size, ComponentTypeCategory.Managed
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(), typeof(Position2D), 2, 8, ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position3D>(), typeof(Position3D), 3, 12, ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation2D>(), typeof(Rotation2D), 4, 4, ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation3D>(), typeof(Rotation3D), 5, 12, ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Scale2D>(), typeof(Scale2D), 6, 8, ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Scale3D>(), typeof(Scale3D), 7, 12, ComponentTypeCategory.Unmanaged
                };
            }
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
