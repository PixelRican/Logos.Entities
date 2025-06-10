// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for more details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityTableTests
    {
        [TestMethod]
        public void AddTest()
        {
            EntityTable table = CreateTable();

            Assert.IsTrue(table.IsReadOnly);
            Assert.ThrowsException<InvalidOperationException>(() => table.Add(new Entity()));

            lock (table.Archetype)
            {
                Assert.IsFalse(table.IsReadOnly);

                for (int i = 1; i <= table.Capacity; i++)
                {
                    table.Add(new Entity(i, 0));
                    Assert.AreEqual(table.Count, i);
                }

                Assert.IsTrue(table.IsFull);
                Assert.ThrowsException<InvalidOperationException>(() => table.Add(new Entity()));
            }
        }

        [TestMethod]
        public void ClearTest()
        {
            EntityTable table = CreateTable();

            Assert.IsTrue(table.IsReadOnly);
            Assert.ThrowsException<InvalidOperationException>(table.Clear);

            lock (table.Archetype)
            {
                while (!table.IsFull)
                {
                    table.Add(new Entity());
                }

                table.Clear();
                Assert.IsTrue(table.IsEmpty);
            }
        }

        [TestMethod]
        public void ConstructorTest()
        {
            EntityTable table = new EntityTable(EntityArchetype.Base);

            Assert.IsTrue(table.IsEmpty);
            Assert.IsFalse(table.IsReadOnly);
            Assert.AreEqual(8, table.Capacity);
            Assert.ThrowsException<ArgumentNullException>(() => new EntityTable(null!));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new EntityTable(EntityArchetype.Base, -1));
        }

        [TestMethod]
        public void RemoveTest()
        {
            EntityTable table = CreateTable();

            Assert.IsTrue(table.IsReadOnly);
            Assert.IsFalse(table.Remove(new Entity()));
            Assert.ThrowsException<InvalidOperationException>(() => table.RemoveAt(0));
            Assert.ThrowsException<InvalidOperationException>(() => table.RemoveAt(-1));

            lock (table.Archetype)
            {
                Assert.IsFalse(table.IsReadOnly);
                Assert.IsFalse(table.Remove(new Entity()));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => table.RemoveAt(0));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => table.RemoveAt(-1));

                for (int i = 1; i <= table.Capacity; i++)
                {
                    table.Add(new Entity(i, 0));
                }

                ReadOnlySpan<Entity> entities = table.GetEntities();

                for (int i = entities.Length; i > 1; i--)
                {
                    table.RemoveAt(0);
                    Assert.AreEqual(new Entity(i, 0), entities[0]);
                }

                table.RemoveAt(0);
                Assert.AreEqual(new Entity(2, 0), entities[0]);
                Assert.IsTrue(table.IsEmpty);
            }
        }

        private static EntityTable CreateTable()
        {
            EntityArchetype archetype = EntityArchetype.Create([ComponentType.TypeOf<User>(),
                                                                ComponentType.TypeOf<Position2D>(),
                                                                ComponentType.TypeOf<Tag>()]);
            return new EntityTable(archetype, archetype, 16);
        }
    }
}
