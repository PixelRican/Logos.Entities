// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public static class EntityTests
    {
        private static IEnumerable CompareEqualsTestCases
        {
            get
            {
                object[] parameters = new object[2];

                parameters[0] = new Entity(0, 0);
                parameters[1] = new Entity(0, 1);

                yield return parameters;

                parameters[0] = new Entity(0, 0);
                parameters[1] = new Entity(1, 0);

                yield return parameters;

                parameters[0] = new Entity(0, 0);
                parameters[1] = new Entity(1, 1);

                yield return parameters;

                parameters[0] = new Entity(0, 0);
                parameters[1] = new Entity(1, -1);

                yield return parameters;

                parameters[0] = new Entity(-1, 0);
                parameters[1] = new Entity(0, 0);

                yield return parameters;

                parameters[0] = new Entity(0, -1);
                parameters[1] = new Entity(0, 0);

                yield return parameters;

                parameters[0] = new Entity(-1, -1);
                parameters[1] = new Entity(0, 0);

                yield return parameters;

                parameters[0] = new Entity(-1, 1);
                parameters[1] = new Entity(0, 0);

                yield return parameters;
            }
        }

        [Test]
        public static void CompareExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new Entity(0, 0).CompareTo(string.Empty);
            });
        }

        [TestCaseSource(nameof(CompareEqualsTestCases))]
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

        [TestCaseSource(nameof(CompareEqualsTestCases))]
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
