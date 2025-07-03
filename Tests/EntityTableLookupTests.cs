// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Linq;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityTableLookupTests
    {
        [Test]
        public void GetGroupingTest()
        {
            EntityTableLookup lookup = new EntityTableLookup();
            ComponentType[] arguments = new ComponentType[8];

            GetGroupingTestHelper(lookup, arguments);
            Assert.That(1, Is.EqualTo(lookup.Count));

            arguments[0] = ComponentType.TypeOf<Position2D>();
            arguments[1] = ComponentType.TypeOf<Rotation2D>();
            arguments[2] = ComponentType.TypeOf<Scale2D>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.That(2, Is.EqualTo(lookup.Count));

            arguments[0] = arguments[3] = ComponentType.TypeOf<Position3D>();
            arguments[1] = arguments[4] = ComponentType.TypeOf<Rotation3D>();
            arguments[2] = arguments[5] = ComponentType.TypeOf<Scale3D>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.That(3, Is.EqualTo(lookup.Count));

            arguments[0] = ComponentType.TypeOf<Name>();
            arguments[1] = ComponentType.TypeOf<Position2D>();
            arguments[2] = ComponentType.TypeOf<Rotation2D>();
            arguments[3] = ComponentType.TypeOf<Scale2D>();
            arguments[4] = ComponentType.TypeOf<Enabled>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.That(4, Is.EqualTo(lookup.Count));

            arguments[0] = ComponentType.TypeOf<Name>();
            arguments[1] = ComponentType.TypeOf<Position2D>();
            arguments[2] = ComponentType.TypeOf<Position3D>();
            arguments[3] = ComponentType.TypeOf<Rotation2D>();
            arguments[4] = ComponentType.TypeOf<Rotation3D>();
            arguments[5] = ComponentType.TypeOf<Scale2D>();
            arguments[6] = ComponentType.TypeOf<Scale3D>();
            arguments[7] = ComponentType.TypeOf<Enabled>();

            GetGroupingTestHelper(lookup, arguments);
            Assert.That(5, Is.EqualTo(lookup.Count));
        }

        private static void GetGroupingTestHelper(EntityTableLookup lookup, ComponentType[] arguments)
        {
            EntityTableGrouping grouping = lookup.GetGrouping(arguments);

            Assert.That(grouping, Is.SameAs(lookup.GetGrouping(grouping.Key)));
            Assert.That(grouping, Is.SameAs(lookup.GetGrouping(arguments)));
            Assert.That(grouping, Is.SameAs(lookup.GetGrouping(arguments.AsEnumerable())));
            Assert.That(grouping, Is.SameAs(lookup.GetGrouping(arguments.AsSpan())));
        }
    }
}
