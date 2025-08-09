// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Logos.Entities.Tests
{
    [TestFixture]
    public static class EntityFilterTestFixture
    {
        [Test]
        public static void CreateExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create((IEnumerable<ComponentType>)null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(null!, Array.Empty<ComponentType>(), Array.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(Array.Empty<ComponentType>(), null!, Array.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(Array.Empty<ComponentType>(), Array.Empty<ComponentType>(), null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(null!, Enumerable.Empty<ComponentType>(), Enumerable.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(Enumerable.Empty<ComponentType>(), null!, Enumerable.Empty<ComponentType>());
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityFilter.Create(Enumerable.Empty<ComponentType>(), Enumerable.Empty<ComponentType>(), null!);
            });
        }

        [TestCaseSource(typeof(EntityFilterTestCaseSource), nameof(EntityFilterTestCaseSource.CreateTestCases))]
        public static void CreateTest(ComponentType[] arguments, ComponentType[] expectedComponentTypes)
        {
            IEnumerable<ComponentType> enumerable = arguments;
            ReadOnlySpan<ComponentType> span = new ReadOnlySpan<ComponentType>(arguments);

            for (int method = 0; method < 6; method++)
            {
                EntityFilter filter = method switch
                {
                    0 => EntityFilter.Create(arguments, arguments, arguments),
                    1 => EntityFilter.Create(enumerable, enumerable, enumerable),
                    2 => EntityFilter.Create(span, span, span),
                    3 => EntityFilter.Require(arguments).Include(enumerable).Exclude(span).Build(),
                    4 => EntityFilter.Require(enumerable).Include(span).Exclude(arguments).Build(),
                    _ => EntityFilter.Require(span).Include(arguments).Exclude(enumerable).Build(),
                };

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(filter.RequiredComponentTypes.SequenceEqual(expectedComponentTypes), Is.True);
                    Assert.That(filter.IncludedComponentTypes.SequenceEqual(expectedComponentTypes), Is.True);
                    Assert.That(filter.ExcludedComponentTypes.SequenceEqual(expectedComponentTypes), Is.True);
                }
            }
        }

        [TestCaseSource(typeof(EntityFilterTestCaseSource), nameof(EntityFilterTestCaseSource.EqualsTestCases))]
        public static void EqualsTest(EntityFilter? source, EntityFilter? other)
        {
            EqualityComparer<EntityFilter> comparer = EqualityComparer<EntityFilter>.Default;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comparer.Equals(source, source), Is.True);
                Assert.That(comparer.Equals(other, other), Is.True);
                Assert.That(comparer.Equals(source, other), Is.False);
                Assert.That(comparer.Equals(other, source), Is.False);
            }
        }

        [Test]
        public static void MatchesTest()
        {
            EntityFilter filter = EntityFilter.Create(
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

                Assert.That(filter.Matches(archetype), Is.True);
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

                Assert.That(filter.Matches(archetype), Is.False);
            }
        }
    }
}
