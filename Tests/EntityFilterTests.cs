// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityFilterTests
    {
        [Test]
        public void CreateTest()
        {
            ComponentType[] expected = new ComponentType[8];
            ComponentType[] actual = new ComponentType[8];

            CreateTestHelper(expected, 0, actual, 0);

            expected[0] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[1] = actual[0] = ComponentType.TypeOf<Rotation2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Scale2D>();

            CreateTestHelper(expected, 3, actual, 3);

            expected[0] = actual[2] = actual[3] = ComponentType.TypeOf<Position3D>();
            expected[1] = actual[1] = actual[4] = ComponentType.TypeOf<Rotation3D>();
            expected[2] = actual[0] = actual[5] = ComponentType.TypeOf<Scale3D>();

            CreateTestHelper(expected, 3, actual, 6);

            expected[0] = actual[4] = ComponentType.TypeOf<Name>();
            expected[1] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Rotation2D>();
            expected[3] = actual[3] = ComponentType.TypeOf<Scale2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Enabled>();

            CreateTestHelper(expected, 5, actual, 5);

            expected[0] = actual[1] = ComponentType.TypeOf<Name>();
            expected[1] = actual[3] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[5] = ComponentType.TypeOf<Position3D>();
            expected[3] = actual[7] = ComponentType.TypeOf<Rotation2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Rotation3D>();
            expected[5] = actual[2] = ComponentType.TypeOf<Scale2D>();
            expected[6] = actual[4] = ComponentType.TypeOf<Scale3D>();
            expected[7] = actual[6] = ComponentType.TypeOf<Enabled>();

            CreateTestHelper(expected, 8, actual, 8);
        }

        private static void CreateTestHelper(ComponentType[] expectedArray,
            int expectedLength, ComponentType[] actualArray, int actualLength)
        {
            IEnumerable<ComponentType> actualEnumerable = actualArray.Take(actualLength);
            ReadOnlySpan<ComponentType> actualSpan = actualArray.AsSpan(0, actualLength);
            ReadOnlySpan<EntityFilter> filters =
            [
                EntityFilter.Create(actualArray, actualArray, actualArray),
                EntityFilter.Create(actualEnumerable, actualEnumerable, actualEnumerable),
                EntityFilter.Create(actualSpan, actualSpan, actualSpan),
                EntityFilter.Require(actualArray).Include(actualEnumerable).Exclude(actualSpan).Build(),
                EntityFilter.Require(actualEnumerable).Include(actualSpan).Exclude(actualArray).Build(),
                EntityFilter.Require(actualSpan).Include(actualArray).Exclude(actualEnumerable).Build()
            ];

            foreach (EntityFilter filter in filters)
            {
                ReadOnlySpan<ComponentType> requiredComponentTypes = filter.RequiredComponentTypes;
                ReadOnlySpan<ComponentType> includedComponentTypes = filter.IncludedComponentTypes;
                ReadOnlySpan<ComponentType> excludedComponentTypes = filter.ExcludedComponentTypes;

                Assert.That(expectedLength, Is.EqualTo(requiredComponentTypes.Length));
                Assert.That(expectedLength, Is.EqualTo(includedComponentTypes.Length));
                Assert.That(expectedLength, Is.EqualTo(excludedComponentTypes.Length));

                for (int i = 0; i < expectedLength; i++)
                {
                    ComponentType componentType = expectedArray[i];

                    Assert.That(filter.Requires(componentType));
                    Assert.That(filter.Includes(componentType));
                    Assert.That(filter.Excludes(componentType));
                    Assert.That(componentType, Is.EqualTo(requiredComponentTypes[i]));
                    Assert.That(componentType, Is.EqualTo(includedComponentTypes[i]));
                    Assert.That(componentType, Is.EqualTo(excludedComponentTypes[i]));
                }
            }

            Array.Clear(expectedArray, 0, expectedLength);
            Array.Clear(actualArray, 0, actualLength);
        }

        [Test]
        public void EquatableTest()
        {
            ReadOnlySpan<ComponentType[]> arguments =
            [
                [
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    ComponentType.TypeOf<Scale2D>()
                ],
                [
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    ComponentType.TypeOf<Scale2D>()
                ],
                [
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    ComponentType.TypeOf<Scale2D>(),
                    ComponentType.TypeOf<Enabled>()
                ],
                [
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Position3D>(),
                    ComponentType.TypeOf<Rotation3D>(),
                    ComponentType.TypeOf<Scale3D>(),
                    ComponentType.TypeOf<Enabled>()
                ]
            ];
            ReadOnlySpan<EntityFilter> span =
            [
                EntityFilter.Universal,
                EntityFilter.Create(arguments[0], arguments[0], arguments[0]),
                EntityFilter.Create(arguments[1], arguments[1], arguments[1]),
                EntityFilter.Create(arguments[2], arguments[2], arguments[2]),
                EntityFilter.Create(arguments[3], arguments[3], arguments[3])
            ];
            EntityFilter previous = null!;

            foreach (EntityFilter current in span)
            {
                Assert.That(current, Is.EqualTo(current));
                Assert.That(current, Is.EqualTo(current.ToBuilder().Build()));
                Assert.That(previous, Is.Not.EqualTo(current));

                previous = current;
            }
        }

        [Test]
        public void MatchesTest()
        {
            EntityFilter filter = EntityFilter.Require([ComponentType.TypeOf<Position2D>(),
                                                        ComponentType.TypeOf<Rotation2D>(),
                                                        ComponentType.TypeOf<Scale2D>()])
                                              .Include([ComponentType.TypeOf<Name>(),
                                                        ComponentType.TypeOf<Enabled>()])
                                              .Exclude([ComponentType.TypeOf<Position3D>(),
                                                        ComponentType.TypeOf<Rotation3D>(),
                                                        ComponentType.TypeOf<Scale3D>()])
                                              .Build();
            ReadOnlySpan<EntityArchetype> matches =
            [
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Name>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Enabled>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Name>(),
                                        ComponentType.TypeOf<Enabled>()])
            ];
            ReadOnlySpan<EntityArchetype> mismatches =
            [
                EntityArchetype.Base,
                EntityArchetype.Create([ComponentType.TypeOf<Enabled>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Name>(),
                                        ComponentType.TypeOf<Position3D>(),
                                        ComponentType.TypeOf<Rotation3D>(),
                                        ComponentType.TypeOf<Scale3D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Enabled>(),
                                        ComponentType.TypeOf<Position3D>()]),

            ];

            foreach (EntityArchetype match in matches)
            {
                Assert.That(filter.Matches(match));
            }

            foreach (EntityArchetype mismatch in mismatches)
            {
                Assert.That(filter.Matches(mismatch), Is.False);
            }
        }
    }
}
