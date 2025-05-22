using System;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityTests
    {
        [TestMethod]
        public void CompareTest()
        {
            ReadOnlySpan<Entity> expectedEntities =
            [
                new Entity(-1, -1),
                new Entity(0, 0),
                new Entity(0, 1),
                new Entity(128, -256),
                new Entity(128, 256)
            ];
            Span<Entity> actualEntities =
            [
                new Entity(128, -256),
                new Entity(-1, -1),
                new Entity(128, 256),
                new Entity(0, 1),
                new Entity(0, 0)
            ];

            actualEntities.Sort();

            for (int i = 0; i < 5; i++)
            {
                Entity actualEntity = actualEntities[i];

                Assert.AreEqual(0, actualEntity.CompareTo(expectedEntities[i]));
                Assert.AreEqual(1, actualEntity.CompareTo(null));
                Assert.ThrowsException<ArgumentException>(() => actualEntity.CompareTo(this));
            }
        }

        [TestMethod]
        public void EqualsTest()
        {
            Entity a = new Entity(0, 0);
            Entity b = new Entity(0, 1);
            Entity c = new Entity(1, 0);
            Entity d = new Entity(1, 1);

            Assert.AreEqual(a, a);
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, c);
            Assert.AreNotEqual(c, d);
            Assert.AreNotEqual(d, a);
        }
    }
}
