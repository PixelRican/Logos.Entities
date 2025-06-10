// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityTests
    {
        [TestMethod]
        public void ComparableTest()
        {
            ReadOnlySpan<Entity> expectedSpan =
            [
                new Entity(-1, -1),
                new Entity(0, 0),
                new Entity(0, 1),
                new Entity(128, -256),
                new Entity(128, 256)
            ];
            Span<Entity> actualSpan =
            [
                new Entity(128, -256),
                new Entity(-1, -1),
                new Entity(128, 256),
                new Entity(0, 1),
                new Entity(0, 0)
            ];

            actualSpan.Sort();

            for (int i = 0; i < 5; i++)
            {
                Entity actual = actualSpan[i];

                Assert.AreEqual(0, actual.CompareTo(expectedSpan[i]));
                Assert.AreEqual(1, actual.CompareTo(null));
                Assert.ThrowsException<ArgumentException>(() => actual.CompareTo(this));
            }
        }

        [TestMethod]
        public void EquatableTest()
        {
            ReadOnlySpan<Entity> span =
            [
                new Entity(0, 0),
                new Entity(0, 1),
                new Entity(1, 0),
                new Entity(1, 1)
            ];
            Entity previous = new Entity(-1, -1);

            foreach (Entity current in span)
            {
                Assert.AreEqual(current, current);
                Assert.AreNotEqual(previous, current);

                previous = current;
            }
        }
    }
}
