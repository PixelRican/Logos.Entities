// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityTests
    {
        [Test]
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

                Assert.That(actual.CompareTo(expectedSpan[i]), Is.EqualTo(0));
                Assert.That(actual.CompareTo(null), Is.EqualTo(1));
                Assert.Throws<ArgumentException>(() => actual.CompareTo(this));
            }
        }

        [Test]
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
                Assert.That(current, Is.EqualTo(current));
                Assert.That(current, Is.Not.EqualTo(previous));

                previous = current;
            }
        }
    }
}
