// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityFilterTests
    {
        [TestMethod]
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

            expected[0] = actual[4] = ComponentType.TypeOf<User>();
            expected[1] = actual[2] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[1] = ComponentType.TypeOf<Rotation2D>();
            expected[3] = actual[3] = ComponentType.TypeOf<Scale2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Tag>();

            CreateTestHelper(expected, 5, actual, 5);

            expected[0] = actual[1] = ComponentType.TypeOf<User>();
            expected[1] = actual[3] = ComponentType.TypeOf<Position2D>();
            expected[2] = actual[5] = ComponentType.TypeOf<Position3D>();
            expected[3] = actual[7] = ComponentType.TypeOf<Rotation2D>();
            expected[4] = actual[0] = ComponentType.TypeOf<Rotation3D>();
            expected[5] = actual[2] = ComponentType.TypeOf<Scale2D>();
            expected[6] = actual[4] = ComponentType.TypeOf<Scale3D>();
            expected[7] = actual[6] = ComponentType.TypeOf<Tag>();

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

                Assert.AreEqual(expectedLength, requiredComponentTypes.Length);
                Assert.AreEqual(expectedLength, includedComponentTypes.Length);
                Assert.AreEqual(expectedLength, excludedComponentTypes.Length);

                for (int i = 0; i < expectedLength; i++)
                {
                    ComponentType componentType = expectedArray[i];

                    Assert.IsTrue(filter.Requires(componentType));
                    Assert.IsTrue(filter.Includes(componentType));
                    Assert.IsTrue(filter.Excludes(componentType));
                    Assert.AreEqual(componentType, requiredComponentTypes[i]);
                    Assert.AreEqual(componentType, includedComponentTypes[i]);
                    Assert.AreEqual(componentType, excludedComponentTypes[i]);
                }
            }

            Array.Clear(expectedArray, 0, expectedLength);
            Array.Clear(actualArray, 0, actualLength);
        }

        [TestMethod]
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
                    ComponentType.TypeOf<User>(),
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    ComponentType.TypeOf<Scale2D>()
                ],
                [
                    ComponentType.TypeOf<User>(),
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>(),
                    ComponentType.TypeOf<Scale2D>(),
                    ComponentType.TypeOf<Tag>()
                ],
                [
                    ComponentType.TypeOf<User>(),
                    ComponentType.TypeOf<Position3D>(),
                    ComponentType.TypeOf<Rotation3D>(),
                    ComponentType.TypeOf<Scale3D>(),
                    ComponentType.TypeOf<Tag>()
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
                Assert.AreEqual(current, current);
                Assert.AreEqual(current, current.ToBuilder().Build());
                Assert.AreNotEqual(previous, current);

                previous = current;
            }
        }

        [TestMethod]
        public void MatchesTest()
        {
            EntityFilter filter = EntityFilter.Require([ComponentType.TypeOf<Position2D>(),
                                                        ComponentType.TypeOf<Rotation2D>(),
                                                        ComponentType.TypeOf<Scale2D>()])
                                              .Include([ComponentType.TypeOf<User>(),
                                                        ComponentType.TypeOf<Tag>()])
                                              .Exclude([ComponentType.TypeOf<Position3D>(),
                                                        ComponentType.TypeOf<Rotation3D>(),
                                                        ComponentType.TypeOf<Scale3D>()])
                                              .Build();
            ReadOnlySpan<EntityArchetype> matches =
            [
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<User>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Tag>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<User>(),
                                        ComponentType.TypeOf<Tag>()])
            ];
            ReadOnlySpan<EntityArchetype> mismatches =
            [
                EntityArchetype.Base,
                EntityArchetype.Create([ComponentType.TypeOf<Tag>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<User>(),
                                        ComponentType.TypeOf<Position3D>(),
                                        ComponentType.TypeOf<Rotation3D>(),
                                        ComponentType.TypeOf<Scale3D>()]),
                EntityArchetype.Create([ComponentType.TypeOf<Position2D>(),
                                        ComponentType.TypeOf<Rotation2D>(),
                                        ComponentType.TypeOf<Scale2D>(),
                                        ComponentType.TypeOf<Tag>(),
                                        ComponentType.TypeOf<Position3D>()]),

            ];

            foreach (EntityArchetype match in matches)
            {
                Assert.IsTrue(filter.Matches(match));
            }

            foreach (EntityArchetype mismatch in mismatches)
            {
                Assert.IsFalse(filter.Matches(mismatch));
            }
        }
    }
}
