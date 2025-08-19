// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Logos.Entities.Tests
{
    [TestFixture, TestOf(typeof(EntityArchetype))]
    public static class EntityArchetypeTestFixture
    {
        private static IEnumerable AddTestCases
        {
            get
            {
                // Add Disabled component type to base archetype.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Disabled>()
                };

                // Add Name component type to base archetype.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Name>()
                };

                // Add Position2D component type to base archetype.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Position2D>()
                };

                // Add Name component type to archetype.
                yield return new object[]
                {
                    CreateArchetype<Disabled>(),
                    ComponentType.TypeOf<Name>()
                };

                // Add Disabled component type to archetype.
                yield return new object[]
                {
                    CreateArchetype<Name>(),
                    ComponentType.TypeOf<Disabled>()
                };

                // Add Name component type to archetype.
                yield return new object[]
                {
                    CreateArchetype<Position2D, Disabled>(),
                    ComponentType.TypeOf<Name>()
                };

                // Add Position2D component type to archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Disabled>(),
                    ComponentType.TypeOf<Position2D>()
                };

                // Add Disabled component type to archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Position2D>(),
                    ComponentType.TypeOf<Disabled>()
                };
            }
        }

        private static IEnumerable CreateTestCases
        {
            get
            {
                // Check throw condition.
                yield return new object[]
                {
                    null!
                };

                // Get base archetype.
                yield return new object[]
                {
                    Array.Empty<ComponentType>()
                };

                // Create archetype with Name component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }
                };

                // Create archetype with Position2D component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        null!
                    }
                };

                // Create archetype with Disabled component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        null!,
                        ComponentType.TypeOf<Disabled>()
                    }
                };

                // Create archetype with Position3D component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Position3D>(),
                        ComponentType.TypeOf<Position3D>()
                    }
                };

                // Create archetype with Rotation2D and Rotation3D component types.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Rotation2D>(),
                        null!,
                        ComponentType.TypeOf<Rotation3D>()
                    }
                };

                // Create archetype with Name, Scale2D, and Disabled component types.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>()
                    }
                };
            }
        }

        private static IEnumerable EqualsTestCases
        {
            get
            {
                // Compare base archetype with null.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    null!,
                    false
                };

                // Compare null with base archetype.
                yield return new object[]
                {
                    null!,
                    EntityArchetype.Base,
                    false
                };

                // Compare base archetype with self.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    EntityArchetype.Base,
                    true
                };

                // Compare separate archetypes that contain the same component type.
                yield return new object[]
                {
                    CreateArchetype<Disabled>(),
                    CreateArchetype<Disabled>(),
                    true
                };

                // Compare base archetype with archetype that contains component type.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    CreateArchetype<Name>(),
                    false
                };

                // Compare separate archetypes that contain the same component type.
                yield return new object[]
                {
                    CreateArchetype<Position2D, Position2D>(),
                    CreateArchetype<Position2D>(),
                    true
                };

                // Compare archetypes that contain the same set of component types.
                yield return new object[]
                {
                    CreateArchetype<Disabled, Position2D, Name>(),
                    CreateArchetype<Name, Position2D, Disabled>(),
                    true
                };

                // Compare archetypes with similar component types but with one subtle difference.
                yield return new object[]
                {
                    CreateArchetype<Name, Position2D, Disabled>(),
                    CreateArchetype<Name, Position3D, Disabled>(),
                    false
                };
            }
        }

        private static IEnumerable RemoveTestCases
        {
            get
            {
                // Remove Name component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Position2D, Disabled>(),
                    ComponentType.TypeOf<Name>()
                };

                // Remove Position2D component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Position2D, Disabled>(),
                    ComponentType.TypeOf<Position2D>()
                };

                // Remove Disabled component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Position2D, Disabled>(),
                    ComponentType.TypeOf<Disabled>()
                };

                // Remove Name component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Position2D>(),
                    ComponentType.TypeOf<Name>()
                };

                // Remove Disabled component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Name, Disabled>(),
                    ComponentType.TypeOf<Disabled>()
                };

                // Remove Name component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Name>(),
                    ComponentType.TypeOf<Name>()
                };

                // Remove Position2D component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Position2D>(),
                    ComponentType.TypeOf<Position2D>()
                };

                // Remove Disabled component type from archetype.
                yield return new object[]
                {
                    CreateArchetype<Disabled>(),
                    ComponentType.TypeOf<Disabled>()
                };
            }
        }

        [TestCaseSource(nameof(AddTestCases))]
        public static void AddTest(EntityArchetype source, ComponentType value)
        {
            EntityArchetype result = source.Add(value);
            ReadOnlySpan<ComponentType> resultComponentTypes = result.ComponentTypes;
            ReadOnlySpan<ComponentType> sourceComponentTypes = source.ComponentTypes;
            int expectedIndex = ~sourceComponentTypes.BinarySearch(value);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultComponentTypes.Slice(0, expectedIndex).SequenceEqual(sourceComponentTypes.Slice(0, expectedIndex)));
                Assert.That(resultComponentTypes[expectedIndex], Is.EqualTo(value));
                Assert.That(resultComponentTypes.Slice(expectedIndex + 1).SequenceEqual(sourceComponentTypes.Slice(expectedIndex)));

                for (int i = 0; i < resultComponentTypes.Length; i++)
                {
                    Assert.That(result.Contains(resultComponentTypes[i]));
                    Assert.That(result.IndexOf(resultComponentTypes[i]), Is.EqualTo(i));
                }

                int expectedManagedComponentCount = source.ManagedComponentCount;
                int expectedUnmanagedComponentCount = source.UnmanagedComponentCount;
                int expectedTagComponentCount = source.TagComponentCount;
                int expectedEntitySize = source.EntitySize;

                switch (value.Category)
                {
                    case ComponentTypeCategory.Managed:
                        expectedManagedComponentCount++;
                        goto default;
                    case ComponentTypeCategory.Unmanaged:
                        expectedUnmanagedComponentCount++;
                        goto default;
                    case ComponentTypeCategory.Tag:
                        expectedTagComponentCount++;
                        break;
                    default:
                        expectedEntitySize += value.Size;
                        break;
                }

                Assert.That(result.ManagedComponentCount, Is.EqualTo(expectedManagedComponentCount));
                Assert.That(result.UnmanagedComponentCount, Is.EqualTo(expectedUnmanagedComponentCount));
                Assert.That(result.TagComponentCount, Is.EqualTo(expectedTagComponentCount));
                Assert.That(result.EntitySize, Is.EqualTo(expectedEntitySize));
            }
        }

        [TestCaseSource(nameof(CreateTestCases))]
        public static void CreateTest(ComponentType[]? array)
        {
            if (array != null)
            {
                AssertHelper(EntityArchetype.Create(array: array));
                AssertHelper(EntityArchetype.Create(collection: array));
                AssertHelper(EntityArchetype.Create(span: array));
            }
            else
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    EntityArchetype.Create(array: array!);
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    EntityArchetype.Create(collection: array!);
                });
            }

            static void AssertHelper(EntityArchetype actual)
            {
                ReadOnlySpan<ComponentType> actualComponentTypes = actual.ComponentTypes;
                int expectedManagedComponentCount = 0;
                int expectedUnmanagedComponentCount = 0;
                int expectedTagComponentCount = 0;
                int expectedEntitySize = Unsafe.SizeOf<Entity>();
                ComponentType previousComponentType = null!;

                for (int i = 0; i < actualComponentTypes.Length; i++)
                {
                    ComponentType currentComponentType = actualComponentTypes[i];

                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(currentComponentType.CompareTo(previousComponentType), Is.EqualTo(1));
                        Assert.That(actual.Contains(currentComponentType));
                        Assert.That(actual.IndexOf(currentComponentType), Is.EqualTo(i));
                    }

                    previousComponentType = currentComponentType;

                    switch (currentComponentType.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            expectedManagedComponentCount++;
                            break;
                        case ComponentTypeCategory.Unmanaged:
                            expectedUnmanagedComponentCount++;
                            break;
                        case ComponentTypeCategory.Tag:
                            expectedTagComponentCount++;
                            continue;
                    }

                    expectedEntitySize += currentComponentType.Size;
                }

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(actual.ManagedComponentCount, Is.EqualTo(expectedManagedComponentCount));
                    Assert.That(actual.UnmanagedComponentCount, Is.EqualTo(expectedUnmanagedComponentCount));
                    Assert.That(actual.TagComponentCount, Is.EqualTo(expectedTagComponentCount));
                    Assert.That(actual.EntitySize, Is.EqualTo(expectedEntitySize));
                }
            }
        }

        [TestCaseSource(nameof(EqualsTestCases))]
        public static void EqualsTest(EntityArchetype? left, EntityArchetype? right, bool expected)
        {
            using (Assert.EnterMultipleScope())
            {
                if (left is not null)
                {
                    Assert.That(left.Equals(right), Is.EqualTo(expected));
                    Assert.That(left.Equals(right as object), Is.EqualTo(expected));
                }

                Assert.That(left == right, Is.EqualTo(expected));
                Assert.That(left != right, Is.Not.EqualTo(expected));

                if (right is not null)
                {
                    Assert.That(right.Equals(left), Is.EqualTo(expected));
                    Assert.That(right.Equals(left as object), Is.EqualTo(expected));
                }

                Assert.That(right == left, Is.EqualTo(expected));
                Assert.That(right != left, Is.Not.EqualTo(expected));
            }
        }

        [TestCaseSource(nameof(RemoveTestCases))]
        public static void RemoveTest(EntityArchetype source, ComponentType value)
        {
            EntityArchetype result = source.Remove(value);
            ReadOnlySpan<ComponentType> resultComponentTypes = result.ComponentTypes;
            ReadOnlySpan<ComponentType> sourceComponentTypes = source.ComponentTypes;
            int expectedIndex = sourceComponentTypes.BinarySearch(value);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultComponentTypes.Slice(0, expectedIndex).SequenceEqual(sourceComponentTypes.Slice(0, expectedIndex)));
                Assert.That(resultComponentTypes.Slice(expectedIndex).SequenceEqual(sourceComponentTypes.Slice(expectedIndex + 1)));

                for (int i = 0; i < resultComponentTypes.Length; i++)
                {
                    Assert.That(result.Contains(resultComponentTypes[i]));
                    Assert.That(result.IndexOf(resultComponentTypes[i]), Is.EqualTo(i));
                }

                int expectedManagedComponentCount = source.ManagedComponentCount;
                int expectedUnmanagedComponentCount = source.UnmanagedComponentCount;
                int expectedTagComponentCount = source.TagComponentCount;
                int expectedEntitySize = source.EntitySize;

                switch (value.Category)
                {
                    case ComponentTypeCategory.Managed:
                        expectedManagedComponentCount--;
                        goto default;
                    case ComponentTypeCategory.Unmanaged:
                        expectedUnmanagedComponentCount--;
                        goto default;
                    case ComponentTypeCategory.Tag:
                        expectedTagComponentCount--;
                        break;
                    default:
                        expectedEntitySize -= value.Size;
                        break;
                }

                Assert.That(result.ManagedComponentCount, Is.EqualTo(expectedManagedComponentCount));
                Assert.That(result.UnmanagedComponentCount, Is.EqualTo(expectedUnmanagedComponentCount));
                Assert.That(result.TagComponentCount, Is.EqualTo(expectedTagComponentCount));
                Assert.That(result.EntitySize, Is.EqualTo(expectedEntitySize));
            }
        }

        private static EntityArchetype CreateArchetype<T>()
        {
            return EntityArchetype.Create([ComponentType.TypeOf<T>()]);
        }

        private static EntityArchetype CreateArchetype<T1, T2>()
        {
            return EntityArchetype.Create([ComponentType.TypeOf<T1>(),
                                           ComponentType.TypeOf<T2>()]);
        }

        private static EntityArchetype CreateArchetype<T1, T2, T3>()
        {
            return EntityArchetype.Create([ComponentType.TypeOf<T1>(),
                                           ComponentType.TypeOf<T2>(),
                                           ComponentType.TypeOf<T3>()]);
        }
    }
}
