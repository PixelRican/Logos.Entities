// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public static class EntityArchetypeTests
    {
        private static IEnumerable AddTestCases
        {
            get
            {
                yield return new object[]
                {
                    EntityArchetype.Base, ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Base, ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Base, ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>()]), ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Name>()
                };
                
                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>()]), ComponentType.TypeOf<Enabled>()
                };
            }
        }

        private static IEnumerable CreateTestCases
        {
            get
            {
                yield return new object[]
                {
                    Array.Empty<ComponentType>(),
                    Array.Empty<ComponentType>()
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        null!
                    },
                    Array.Empty<ComponentType>()
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(), null!
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        null!, ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(), null!, ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Enabled>()
                    }
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Enabled>()
                    }
                };
            }
        }

        private static IEnumerable EqualsTestCases
        {
            get
            {
                yield return new object[]
                {
                    EntityArchetype.Base, null!
                };

                yield return new object[]
                {
                    EntityArchetype.Base, EntityArchetype.Create([ComponentType.TypeOf<Enabled>()])
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Name>()]), EntityArchetype.Create([ComponentType.TypeOf<Enabled>()])
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Name>()]), EntityArchetype.Create([ComponentType.TypeOf<Name>()])
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Name>()]), EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Position2D>()])
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Name>()]), EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Rotation2D>()])
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Name>()]), EntityArchetype.Create([ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Rotation2D>()])
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Name>()]), EntityArchetype.Create([ComponentType.TypeOf<Enabled>(), ComponentType.TypeOf<Position3D>(), ComponentType.TypeOf<Name>()])
                };
            }
        }

        private static IEnumerable RemoveTestCases
        {
            get
            {
                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Position2D>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>(), ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Name>()]), ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Position2D>()]), ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>()]), ComponentType.TypeOf<Enabled>()
                };
            }
        }

        [TestCaseSource(nameof(AddTestCases))]
        public static void AddTest(EntityArchetype archetype, ComponentType type)
        {
            EntityArchetype result = archetype.Add(type);
            ReadOnlySpan<ComponentType> subset = archetype.ComponentTypes;
            ReadOnlySpan<ComponentType> superset = result.ComponentTypes;
            int index = ~subset.BinarySearch(type);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(superset.BinarySearch(type), Is.EqualTo(index));
                Assert.That(archetype.Contains(type), Is.False);
                Assert.That(result.Contains(type), Is.True);
                Assert.That(subset.Slice(0, index).SequenceEqual(superset.Slice(0, index)), Is.True);
                Assert.That(subset.Slice(index).SequenceEqual(superset.Slice(index + 1)), Is.True);
                Assert.That(archetype, Is.SameAs(archetype.Add(null!)));
                Assert.That(result, Is.SameAs(result.Add(type)));
            }
        }

        [TestCaseSource(nameof(CreateTestCases))]
        public static void CreateTest(ComponentType[] arguments, ComponentType[] expectedTypes)
        {
            int expectedManagedComponentCount = 0;
            int expectedUnmanagedComponentCount = 0;
            int expectedTagComponentCount = 0;
            int expectedEntitySize = 8;

            foreach (ComponentType type in expectedTypes)
            {
                expectedEntitySize += type.Size;

                switch (type.Category)
                {
                    case ComponentTypeCategory.Managed:
                        expectedManagedComponentCount++;
                        continue;
                    case ComponentTypeCategory.Unmanaged:
                        expectedUnmanagedComponentCount++;
                        continue;
                    case ComponentTypeCategory.Tag:
                        expectedTagComponentCount++;
                        continue;
                }
            }

            for (int method = 0; method < 3; method++)
            {
                EntityArchetype actual = method switch
                {
                    0 => EntityArchetype.Create(arguments),
                    1 => EntityArchetype.Create((IEnumerable<ComponentType>)arguments),
                    _ => EntityArchetype.Create((ReadOnlySpan<ComponentType>)arguments)
                };

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(actual.ComponentTypes.SequenceEqual(expectedTypes), Is.True);
                    Assert.That(actual.ManagedComponentCount, Is.EqualTo(expectedManagedComponentCount));
                    Assert.That(actual.UnmanagedComponentCount, Is.EqualTo(expectedUnmanagedComponentCount));
                    Assert.That(actual.TagComponentCount, Is.EqualTo(expectedTagComponentCount));
                    Assert.That(actual.EntitySize, Is.EqualTo(expectedEntitySize));
                }
            }
        }

        [TestCaseSource(nameof(EqualsTestCases))]
        public static void EqualsTest(EntityArchetype? source, EntityArchetype? other)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(EntityArchetype.Equals(source, source), Is.True);
                Assert.That(EntityArchetype.Equals(other, other), Is.True);
                Assert.That(EntityArchetype.Equals(source, other), Is.False);
                Assert.That(EntityArchetype.Equals(other, source), Is.False);
            }
        }

        [TestCaseSource(nameof(RemoveTestCases))]
        public static void RemoveTest(EntityArchetype archetype, ComponentType type)
        {
            EntityArchetype result = archetype.Remove(type);
            ReadOnlySpan<ComponentType> subset = result.ComponentTypes;
            ReadOnlySpan<ComponentType> superset = archetype.ComponentTypes;
            int index = superset.BinarySearch(type);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(subset.BinarySearch(type), Is.EqualTo(~index));
                Assert.That(archetype.Contains(type), Is.True);
                Assert.That(result.Contains(type), Is.False);
                Assert.That(subset.Slice(0, index).SequenceEqual(superset.Slice(0, index)), Is.True);
                Assert.That(subset.Slice(index).SequenceEqual(superset.Slice(index + 1)), Is.True);
                Assert.That(archetype, Is.SameAs(archetype.Remove(null!)));
                Assert.That(result, Is.SameAs(result.Remove(type)));
            }
        }
    }
}
