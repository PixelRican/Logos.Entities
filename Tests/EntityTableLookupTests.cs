// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityTableLookupTests
    {
        [TestMethod]
        public void GetGroupingTest()
        {
            EntityTableLookup lookup = new();
            ComponentType[] arguments = new ComponentType[8];

            GetGroupingTestHelper(lookup, arguments);
            Assert.AreEqual(1, lookup.Count);

            arguments[0] = ComponentType.TypeOf<Position2D>();
            arguments[1] = ComponentType.TypeOf<Rotation2D>();
            arguments[2] = ComponentType.TypeOf<Scale2D>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.AreEqual(2, lookup.Count);

            arguments[0] = arguments[3] = ComponentType.TypeOf<Position3D>();
            arguments[1] = arguments[4] = ComponentType.TypeOf<Rotation3D>();
            arguments[2] = arguments[5] = ComponentType.TypeOf<Scale3D>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.AreEqual(3, lookup.Count);

            arguments[0] = ComponentType.TypeOf<User>();
            arguments[1] = ComponentType.TypeOf<Position2D>();
            arguments[2] = ComponentType.TypeOf<Rotation2D>();
            arguments[3] = ComponentType.TypeOf<Scale2D>();
            arguments[4] = ComponentType.TypeOf<Tag>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.AreEqual(4, lookup.Count);

            arguments[0] = ComponentType.TypeOf<User>();
            arguments[1] = ComponentType.TypeOf<Position2D>();
            arguments[2] = ComponentType.TypeOf<Position3D>();
            arguments[3] = ComponentType.TypeOf<Rotation2D>();
            arguments[4] = ComponentType.TypeOf<Rotation3D>();
            arguments[5] = ComponentType.TypeOf<Scale2D>();
            arguments[6] = ComponentType.TypeOf<Scale3D>();
            arguments[7] = ComponentType.TypeOf<Tag>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.AreEqual(5, lookup.Count);
        }

        private static void GetGroupingTestHelper(EntityTableLookup lookup, ComponentType[] arguments)
        {
            EntityTableGrouping grouping = lookup.GetGrouping(arguments);

            Assert.AreSame(grouping, lookup.GetGrouping(grouping.Key));
            Assert.AreSame(grouping, lookup.GetGrouping(arguments));
            Assert.AreSame(grouping, lookup.GetGrouping(arguments.AsEnumerable()));
            Assert.AreSame(grouping, lookup.GetGrouping(arguments.AsSpan()));
        }
    }
}
