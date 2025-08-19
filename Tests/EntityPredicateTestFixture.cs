// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Logos.Entities.Tests
{
    [TestFixture, TestOf(typeof(EntityPredicate))]
    public static class EntityPredicateTestFixture
    {
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
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        null!
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                };

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

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Disabled>()
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
                    EntityPredicate.Universal,
                    null!,
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Universal,
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>()),
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Universal,
                    EntityPredicate.Create(
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        Array.Empty<ComponentType>()),
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Universal,
                    EntityPredicate.Create(
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        }),
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        }),
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    false
                };

                yield return new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        }),
                    false
                };
            }
        }

        [Test]
        public static void CreateExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(requiredArray: null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(requiredCollection: null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(null!, Array.Empty<ComponentType>(), Array.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(Array.Empty<ComponentType>(), null!, Array.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(Array.Empty<ComponentType>(), Array.Empty<ComponentType>(), null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(null!, Enumerable.Empty<ComponentType>(), Enumerable.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(Enumerable.Empty<ComponentType>(), null!, Enumerable.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityPredicate.Create(Enumerable.Empty<ComponentType>(), Enumerable.Empty<ComponentType>(), null!);
            });
        }

        [TestCaseSource(nameof(CreateTestCases))]
        public static void CreateTest(ComponentType[] arguments, ComponentType[] expectedComponentTypes)
        {
            IEnumerable<ComponentType> collection = arguments;
            ReadOnlySpan<ComponentType> span = new ReadOnlySpan<ComponentType>(arguments);

            for (int method = 0; method < 6; method++)
            {
                EntityPredicate predicate = method switch
                {
                    0 => EntityPredicate.Create(arguments, arguments, arguments),
                    1 => EntityPredicate.Create(collection, collection, collection),
                    2 => EntityPredicate.Create(span, span, span),
                    3 => EntityPredicate.Require(arguments).Include(collection).Exclude(span).Construct(),
                    4 => EntityPredicate.Require(collection).Include(span).Exclude(arguments).Construct(),
                    _ => EntityPredicate.Require(span).Include(arguments).Exclude(collection).Construct(),
                };

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(predicate.RequiredComponentTypes.SequenceEqual(expectedComponentTypes));
                    Assert.That(predicate.IncludedComponentTypes.SequenceEqual(expectedComponentTypes));
                    Assert.That(predicate.ExcludedComponentTypes.SequenceEqual(expectedComponentTypes));
                }
            }
        }

        [TestCaseSource(nameof(EqualsTestCases))]
        public static void EqualsTest(EntityPredicate? left, EntityPredicate? right, bool expected)
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

        [Test]
        public static void MatchesTest()
        {
            EntityPredicate predicate = EntityPredicate.Create(
                new ComponentType[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    ComponentType.TypeOf<Scale2D>()
                },
                new ComponentType[]
                {
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Disabled>()
                },
                new ComponentType[]
                {
                    ComponentType.TypeOf<Position3D>(),
                    ComponentType.TypeOf<Rotation3D>(),
                    ComponentType.TypeOf<Scale3D>()
                });

            for (int match = 0; match < 3; match++)
            {
                EntityArchetype archetype = match switch
                {
                    0 => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    1 => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }),
                    _ => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    })
                };

                Assert.That(predicate.Matches(archetype));
            }

            for (int mismatch = 0; mismatch < 5; mismatch++)
            {
                EntityArchetype archetype = mismatch switch
                {
                    0 => EntityArchetype.Base,
                    1 => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }),
                    2 => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>(),
                        ComponentType.TypeOf<Scale2D>()
                    }),
                    3 => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position3D>(),
                        ComponentType.TypeOf<Rotation3D>(),
                        ComponentType.TypeOf<Scale3D>()
                    }),
                    _ => EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Position3D>()
                    })
                };

                Assert.That(predicate.Matches(archetype), Is.False);
            }
        }
    }
}
