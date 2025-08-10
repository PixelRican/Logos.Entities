// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace Logos.Entities.Tests
{
    [TestFixture]
    public static class EntityArchetypeTestFixture
    {
        [TestCaseSource(typeof(EntityArchetypeTestCaseSource), nameof(EntityArchetypeTestCaseSource.AddTestCases))]
        public static void AddTest(EntityArchetype archetype, ComponentType componentType)
        {
            EntityArchetype result = archetype.Add(componentType);
            ReadOnlySpan<ComponentType> subset = archetype.ComponentTypes;
            ReadOnlySpan<ComponentType> superset = result.ComponentTypes;
            int index = ~subset.BinarySearch(componentType);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(superset.BinarySearch(componentType), Is.EqualTo(index));
                Assert.That(archetype.Contains(componentType), Is.False);
                Assert.That(archetype.IndexOf(componentType), Is.EqualTo(-1));
                Assert.That(result.Contains(componentType), Is.True);
                Assert.That(result.IndexOf(componentType), Is.EqualTo(index));
                Assert.That(subset.Slice(0, index).SequenceEqual(superset.Slice(0, index)), Is.True);
                Assert.That(subset.Slice(index).SequenceEqual(superset.Slice(index + 1)), Is.True);
                Assert.That(archetype, Is.SameAs(archetype.Add(null!)));
                Assert.That(result, Is.SameAs(result.Add(componentType)));
            }
        }

        [Test]
        public static void CreateExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityArchetype.Create(null!);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                EntityArchetype.Create((IEnumerable<ComponentType>)null!);
            });
        }

        [TestCaseSource(typeof(EntityArchetypeTestCaseSource), nameof(EntityArchetypeTestCaseSource.CreateTestCases))]
        public static void CreateTest(ComponentType[] arguments, ComponentType[] expectedComponentTypes)
        {
            int expectedManagedComponentCount = 0;
            int expectedUnmanagedComponentCount = 0;
            int expectedTagComponentCount = 0;
            int expectedEntitySize = 8;

            foreach (ComponentType type in expectedComponentTypes)
            {
                switch (type.Category)
                {
                    case ComponentTypeCategory.Managed:
                        expectedManagedComponentCount++;
                        break;
                    case ComponentTypeCategory.Unmanaged:
                        expectedUnmanagedComponentCount++;
                        break;
                    case ComponentTypeCategory.Tag:
                        expectedTagComponentCount++;
                        continue;
                }

                expectedEntitySize += type.Size;
            }

            for (int method = 0; method < 3; method++)
            {
                EntityArchetype actual = method switch
                {
                    0 => EntityArchetype.Create(arguments),
                    1 => EntityArchetype.Create((IEnumerable<ComponentType>)arguments),
                    _ => EntityArchetype.Create(new ReadOnlySpan<ComponentType>(arguments))
                };

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(actual.ComponentTypes.SequenceEqual(expectedComponentTypes), Is.True);
                    Assert.That(actual.ManagedComponentCount, Is.EqualTo(expectedManagedComponentCount));
                    Assert.That(actual.UnmanagedComponentCount, Is.EqualTo(expectedUnmanagedComponentCount));
                    Assert.That(actual.TagComponentCount, Is.EqualTo(expectedTagComponentCount));
                    Assert.That(actual.EntitySize, Is.EqualTo(expectedEntitySize));
                }
            }
        }

        [TestCaseSource(typeof(EntityArchetypeTestCaseSource), nameof(EntityArchetypeTestCaseSource.EqualsTestCases))]
        public static void EqualsTest(EntityArchetype? source, EntityArchetype? other)
        {
            EqualityComparer<EntityArchetype> comparer = EqualityComparer<EntityArchetype>.Default;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comparer.Equals(source, source), Is.True);
                Assert.That(comparer.Equals(other, other), Is.True);
                Assert.That(comparer.Equals(source, other), Is.False);
                Assert.That(comparer.Equals(other, source), Is.False);
            }
        }

        [TestCaseSource(typeof(EntityArchetypeTestCaseSource), nameof(EntityArchetypeTestCaseSource.RemoveTestCases))]
        public static void RemoveTest(EntityArchetype archetype, ComponentType componentType)
        {
            EntityArchetype result = archetype.Remove(componentType);
            ReadOnlySpan<ComponentType> subset = result.ComponentTypes;
            ReadOnlySpan<ComponentType> superset = archetype.ComponentTypes;
            int index = superset.BinarySearch(componentType);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(subset.BinarySearch(componentType), Is.EqualTo(~index));
                Assert.That(archetype.Contains(componentType), Is.True);
                Assert.That(archetype.IndexOf(componentType), Is.EqualTo(index));
                Assert.That(result.Contains(componentType), Is.False);
                Assert.That(result.IndexOf(componentType), Is.EqualTo(-1));
                Assert.That(subset.Slice(0, index).SequenceEqual(superset.Slice(0, index)), Is.True);
                Assert.That(subset.Slice(index).SequenceEqual(superset.Slice(index + 1)), Is.True);
                Assert.That(archetype, Is.SameAs(archetype.Remove(null!)));
                Assert.That(result, Is.SameAs(result.Remove(componentType)));
            }
        }
    }
}
