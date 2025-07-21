// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System.Collections.Generic;

namespace Logos.Entities.Tests
{
    [TestFixture]
    public static class EntityTestFixture
    {
        [TestCaseSource(typeof(EntityTestCaseSource), nameof(EntityTestCaseSource.EqualsTestCases))]
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
