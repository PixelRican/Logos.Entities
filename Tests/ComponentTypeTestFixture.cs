// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace Logos.Entities.Tests
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

        [TestCaseSource(typeof(ComponentTypeTestCaseSource), nameof(ComponentTypeTestCaseSource.CompareTestCases))]
        public static void CompareTest(ComponentType? lesser, ComponentType? greater)
        {
            Comparer<ComponentType> comparer = Comparer<ComponentType>.Default;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comparer.Compare(lesser, lesser), Is.Zero);
                Assert.That(comparer.Compare(greater, greater), Is.Zero);
                Assert.That(comparer.Compare(lesser, greater), Is.EqualTo(-1));
                Assert.That(comparer.Compare(greater, lesser), Is.EqualTo(1));
            }
        }

        [TestCaseSource(typeof(ComponentTypeTestCaseSource), nameof(ComponentTypeTestCaseSource.TypeOfTestCases))]
        public static void TypeOfTest(ComponentType actual, Type expectedType,
            int expectedId, int expectedSize, ComponentTypeCategory expectedCategory)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Type, Is.EqualTo(expectedType));
                Assert.That(actual.TypeId, Is.EqualTo(expectedId));
                Assert.That(actual.Size, Is.EqualTo(expectedSize));
                Assert.That(actual.Category, Is.EqualTo(expectedCategory));
            }
        }
    }
}
