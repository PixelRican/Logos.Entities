// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Logos.Entities.Tests
{
    [TestFixture, TestOf(typeof(ComponentType))]
    public static class ComponentTypeTestFixture
    {
        static ComponentTypeTestFixture()
        {
            // Declare component types in a predefined order for test consistency.
            ComponentType.TypeOf<Disabled>();
            ComponentType.TypeOf<Name>();
            ComponentType.TypeOf<Position2D>();
            ComponentType.TypeOf<Position3D>();
            ComponentType.TypeOf<Rotation2D>();
            ComponentType.TypeOf<Rotation3D>();
            ComponentType.TypeOf<Scale2D>();
            ComponentType.TypeOf<Scale3D>();
        }

        private static IEnumerable<object[]> CompareToTestCases
        {
            get
            {
                // Compare instance with null.
                yield return new object[]
                {
                    ComponentType.TypeOf<Disabled>(),
                    null!,
                    1
                };

                // Compare null with instance.
                yield return new object[]
                {
                    null!,
                    ComponentType.TypeOf<Disabled>(),
                    -1
                };

                // Compare instance with self.
                yield return new object[]
                {
                    ComponentType.TypeOf<Disabled>(),
                    ComponentType.TypeOf<Disabled>(),
                    0
                };

                // Compare managed component type with unmanaged component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Position2D>(),
                    -1
                };

                // Compare managed component type with tag component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Disabled>(),
                    -1
                };

                // Compare unmanaged component type with tag component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Disabled>(),
                    -1
                };

                // Compare different component types with same category.
                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Position3D>(),
                    -1
                };

                // Compare different component types with same category.
                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation3D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    1
                };
            }
        }

        private static IEnumerable<object[]> TypeOfTestCases
        {
            get
            {
                // Test Disabled component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Disabled>(),
                    typeof(Disabled),
                    0,
                    Unsafe.SizeOf<Disabled>(),
                    ComponentTypeCategory.Tag
                };

                // Test Name component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(),
                    typeof(Name),
                    1,
                    Unsafe.SizeOf<Name>(),
                    ComponentTypeCategory.Managed
                };

                // Test Position2D component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    typeof(Position2D),
                    2,
                    Unsafe.SizeOf<Position2D>(),
                    ComponentTypeCategory.Unmanaged
                };

                // Test Position3D component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Position3D>(),
                    typeof(Position3D),
                    3,
                    Unsafe.SizeOf<Position3D>(),
                    ComponentTypeCategory.Unmanaged
                };

                // Test Rotation2D component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation2D>(),
                    typeof(Rotation2D),
                    4,
                    Unsafe.SizeOf<Rotation2D>(),
                    ComponentTypeCategory.Unmanaged
                };

                // Test Rotation3D component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation3D>(),
                    typeof(Rotation3D),
                    5,
                    Unsafe.SizeOf<Rotation3D>(),
                    ComponentTypeCategory.Unmanaged
                };

                // Test Scale2D component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Scale2D>(),
                    typeof(Scale2D),
                    6,
                    Unsafe.SizeOf<Scale2D>(),
                    ComponentTypeCategory.Unmanaged
                };

                // Test Scale3D component type.
                yield return new object[]
                {
                    ComponentType.TypeOf<Scale3D>(),
                    typeof(Scale3D),
                    7,
                    Unsafe.SizeOf<Scale3D>(),
                    ComponentTypeCategory.Unmanaged
                };
            }
        }

        [Test]
        public static void CompareToExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ComponentType.TypeOf<Name>().CompareTo(string.Empty);
            });
        }

        [TestCaseSource(nameof(CompareToTestCases))]
        public static void CompareToTest(ComponentType? left, ComponentType? right, int expectedValue)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(left?.CompareTo(right) ?? -1, Is.EqualTo(expectedValue));
                Assert.That(left?.CompareTo(right as object) ?? -1, Is.EqualTo(expectedValue));
            }
        }

        [TestCaseSource(nameof(TypeOfTestCases))]
        public static void TypeOfTest(ComponentType actual, Type expectedType,
            int expectedIndex, int expectedSize, ComponentTypeCategory expectedCategory)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Type, Is.EqualTo(expectedType));
                Assert.That(actual.Index, Is.EqualTo(expectedIndex));
                Assert.That(actual.Size, Is.EqualTo(expectedSize));
                Assert.That(actual.Category, Is.EqualTo(expectedCategory));
            }
        }
    }
}
