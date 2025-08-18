// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Logos.Entities.Tests
{
    [TestFixture, TestOf(typeof(EntityArchetype))]
    public static class EntityArchetypeTestFixture
    {
        private static IEnumerable<object[]> AddTestCases
        {
            get
            {
                // Add tag component type to base archetype.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Disabled>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Add managed component type to base archetype.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Name>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    })
                };

                // Add unmanaged component type to base archetype.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Position2D>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    })
                };

                // Add managed component type to archetype with tag component type.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Name>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Add tag component type to archetype with managed component type.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    ComponentType.TypeOf<Disabled>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Add managed component type to archetype with unmanaged and tag component types.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Name>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Add unmanaged component type to archetype with managed and tag component types.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Position2D>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Add tag component type to archetype with managed and unmanaged component types.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>()
                    }),
                    ComponentType.TypeOf<Disabled>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };
            }
        }

        private static IEnumerable<object[]> CreateTestCases
        {
            get
            {
                // Get base archetype.
                yield return new object[]
                {
                    Array.Empty<ComponentType>(),
                    Array.Empty<ComponentType>()
                };

                // Get base archetype.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        null!
                    },
                    Array.Empty<ComponentType>()
                };

                // Create archetype with Name component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    },
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
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    }
                };

                // Create archetype with Disabled component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        null!,
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                };

                // Create archetype with Disabled component type.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                };

                // Create archetype with Name and Disabled component types.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        null!,
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    }
                };

                // Create archetype with Name, Position2D, and Disabled component types.
                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }
                };
            }
        }

        private static IEnumerable<object[]> EqualsTestCases
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
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }),
                    true
                };

                // Compare base archetype with archetype that contains component type.
                yield return new object[]
                {
                    EntityArchetype.Base,
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    false
                };

                // Compare separate archetypes that contain the same component type.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Position2D>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    }),
                    true
                };

                // Compare archetypes that contain the same set of component types.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    true
                };

                // Compare archetypes with similar component types but with one subtle difference.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Position3D>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    false
                };
            }
        }

        private static IEnumerable<object[]> RemoveTestCases
        {
            get
            {
                // Remove Name component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Name>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Remove Position2D component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Position2D>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Remove Disabled component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Disabled>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>()
                    })
                };

                // Remove Name component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Name>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                // Remove Disabled component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Disabled>(),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    })
                };

                // Remove Name component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    ComponentType.TypeOf<Name>(),
                    EntityArchetype.Base
                };

                // Remove Position2D component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    }),
                    ComponentType.TypeOf<Position2D>(),
                    EntityArchetype.Base
                };

                // Remove Disabled component type from archetype.
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }),
                    ComponentType.TypeOf<Disabled>(),
                    EntityArchetype.Base
                };
            }
        }

        [TestCaseSource(nameof(AddTestCases))]
        public static void AddTest(EntityArchetype archetype, ComponentType componentType, EntityArchetype expected)
        {
            AssertDeepEquality(archetype.Add(componentType), expected);
            Assert.That(expected.Add(componentType), Is.SameAs(expected));
        }

        [Test]
        public static void CreateExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityArchetype.Create(array: null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityArchetype.Create(collection: null!);
            });
        }

        [TestCaseSource(nameof(CreateTestCases))]
        public static void CreateTest(ComponentType[] arguments, ComponentType[] expectedComponentTypes)
        {
            int expectedManagedComponentCount = 0;
            int expectedUnmanagedComponentCount = 0;
            int expectedTagComponentCount = 0;
            int expectedEntitySize = Unsafe.SizeOf<Entity>();

            foreach (ComponentType componentType in expectedComponentTypes)
            {
                switch (componentType.Category)
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

                expectedEntitySize += componentType.Size;
            }

            for (int method = 0; method < 3; method++)
            {
                EntityArchetype actual = method switch
                {
                    0 => EntityArchetype.Create(array: arguments),
                    1 => EntityArchetype.Create(collection: arguments),
                    _ => EntityArchetype.Create(span: arguments)
                };

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(actual.ComponentTypes.SequenceEqual(expectedComponentTypes), Is.True);
                    Assert.That(actual.ManagedComponentCount, Is.EqualTo(expectedManagedComponentCount));
                    Assert.That(actual.UnmanagedComponentCount, Is.EqualTo(expectedUnmanagedComponentCount));
                    Assert.That(actual.TagComponentCount, Is.EqualTo(expectedTagComponentCount));
                    Assert.That(actual.EntitySize, Is.EqualTo(expectedEntitySize));
                }
            }
        }

        [TestCaseSource(nameof(EqualsTestCases))]
        public static void EqualsTest(EntityArchetype? source, EntityArchetype? target, bool expectedValue)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(source?.Equals(target) ?? false, Is.EqualTo(expectedValue));
                Assert.That(source == target, Is.EqualTo(expectedValue));
                Assert.That(source != target, Is.Not.EqualTo(expectedValue));
            }
        }

        [TestCaseSource(nameof(RemoveTestCases))]
        public static void RemoveTest(EntityArchetype archetype, ComponentType componentType, EntityArchetype expected)
        {
            AssertDeepEquality(archetype.Remove(componentType), expected);
            Assert.That(expected.Remove(componentType), Is.SameAs(expected));
        }

        private static void AssertDeepEquality(EntityArchetype actual, EntityArchetype expected)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.ComponentCount, Is.EqualTo(expected.ComponentCount));
                Assert.That(actual.ManagedComponentCount, Is.EqualTo(expected.ManagedComponentCount));
                Assert.That(actual.UnmanagedComponentCount, Is.EqualTo(expected.UnmanagedComponentCount));
                Assert.That(actual.TagComponentCount, Is.EqualTo(expected.TagComponentCount));
                Assert.That(actual.EntitySize, Is.EqualTo(expected.EntitySize));
                Assert.That(actual.ComponentTypes.SequenceEqual(expected.ComponentTypes));
                Assert.That(actual.ComponentBitmap.SequenceEqual(expected.ComponentBitmap));
            }
        }
    }
}
