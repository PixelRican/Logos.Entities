// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System.Collections;

namespace Logos.Entities.Tests
{
    [TestFixture]
    public static class EntityTestFixture
    {
        private static IEnumerable EqualsTestCases
        {
            get
            {
                // Compare entities with same indices and versions.
                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(0, 0),
                    true
                };

                // Compare entities with different indices and versions.
                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, 1),
                    false
                };

                // Compare entities with same indices and different versions.
                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(0, 1),
                    false
                };

                // Compare entities with different indices and same versions.
                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, 0),
                    false
                };
            }
        }

        [TestCaseSource(nameof(EqualsTestCases))]
        public static void EqualsTest(Entity left, Entity right, bool expected)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(left.Equals(right), Is.EqualTo(expected));
                Assert.That(left.Equals(right as object), Is.EqualTo(expected));
                Assert.That(left == right, Is.EqualTo(expected));
                Assert.That(left != right, Is.Not.EqualTo(expected));

                if (expected)
                {
                    Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
                }
            }
        }
    }
}
