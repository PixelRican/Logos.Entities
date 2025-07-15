// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public static class ComponentTypeTestFixture
    {
        [Test]
        public static void CompareExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ComponentType.TypeOf<Name>().CompareTo(string.Empty);
            });
        }

        [TestCaseSource(typeof(ComponentTypeTestCaseSource), nameof(ComponentTypeTestCaseSource.CompareEqualsTestCases))]
        public static void CompareTest(ComponentType? lesser, ComponentType? greater)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ComponentType.Compare(lesser, lesser), Is.Zero);
                Assert.That(ComponentType.Compare(greater, greater), Is.Zero);
                Assert.That(ComponentType.Compare(lesser, greater), Is.EqualTo(-1));
                Assert.That(ComponentType.Compare(greater, lesser), Is.EqualTo(1));
            }
        }

        [TestCaseSource(typeof(ComponentTypeTestCaseSource), nameof(ComponentTypeTestCaseSource.CompareEqualsTestCases))]
        public static void EqualsTest(ComponentType? source, ComponentType? other)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ComponentType.Equals(source, source), Is.True);
                Assert.That(ComponentType.Equals(other, other), Is.True);
                Assert.That(ComponentType.Equals(source, other), Is.False);
                Assert.That(ComponentType.Equals(other, source), Is.False);
            }
        }

        [TestCaseSource(typeof(ComponentTypeTestCaseSource), nameof(ComponentTypeTestCaseSource.TypeOfTestCases))]
        public static void TypeOfTest(ComponentType actual, Type expectedType,
            int expectedId, int expectedSize, ComponentTypeCategory expectedCategory)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Type, Is.EqualTo(expectedType));
                Assert.That(actual.Id, Is.EqualTo(expectedId));
                Assert.That(actual.Size, Is.EqualTo(expectedSize));
                Assert.That(actual.Category, Is.EqualTo(expectedCategory));
            }
        }
    }
}
