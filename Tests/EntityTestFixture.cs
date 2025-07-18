// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace Logos.Entities.Tests
{
    [TestFixture]
    public static class EntityTestFixture
    {
        [Test]
        public static void CompareExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new Entity(0, 0).CompareTo(string.Empty);
            });
        }

        [TestCaseSource(typeof(EntityTestCaseSource), nameof(EntityTestCaseSource.CompareEqualsTestCases))]
        public static void CompareTest(Entity lesser, Entity greater)
        {
            Comparer<Entity> comparer = Comparer<Entity>.Default;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comparer.Compare(lesser, lesser), Is.Zero);
                Assert.That(comparer.Compare(greater, greater), Is.Zero);
                Assert.That(comparer.Compare(lesser, greater), Is.EqualTo(-1));
                Assert.That(comparer.Compare(greater, lesser), Is.EqualTo(1));
            }
        }

        [TestCaseSource(typeof(EntityTestCaseSource), nameof(EntityTestCaseSource.CompareEqualsTestCases))]
        public static void EqualsTest(Entity source, Entity other)
        {
            EqualityComparer<Entity> comparer = EqualityComparer<Entity>.Default;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comparer.Equals(source, source), Is.True);
                Assert.That(comparer.Equals(other, other), Is.True);
                Assert.That(comparer.Equals(source, other), Is.False);
                Assert.That(comparer.Equals(other, source), Is.False);
            }
        }
    }
}
