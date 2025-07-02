// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityTableTests
    {
        [Test]
        public void AddTest()
        {
            EntityTable table = CreateTable();
            Assert.IsTrue(table.CheckAccess());

            for (int i = 1; i <= table.Capacity; i++)
            {
                table.Add(new Entity(i, 0));
                Assert.AreEqual(table.Count, i);
            }

            Assert.IsTrue(table.IsFull);
            Assert.Throws<InvalidOperationException>(() => table.Add(new Entity()));
        }

        [Test]
        public void ClearTest()
        {
            EntityTable table = CreateTable();
            Assert.IsTrue(table.CheckAccess());

            while (!table.IsFull)
            {
                table.Add(new Entity());
            }

            table.Clear();
            Assert.IsTrue(table.IsEmpty);
        }

        [Test]
        public void ConstructorTest()
        {
            EntityTable table = new EntityTable(EntityArchetype.Base);

            Assert.IsTrue(table.IsEmpty);
            Assert.IsTrue(table.CheckAccess());
            Assert.AreEqual(8, table.Capacity);
            Assert.Throws<ArgumentNullException>(() => new EntityTable(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EntityTable(EntityArchetype.Base, -1));
        }

        [Test]
        public void RemoveTest()
        {
            EntityTable table = CreateTable();

            Assert.IsTrue(table.CheckAccess());
            Assert.IsFalse(table.Remove(new Entity()));
            Assert.Throws<ArgumentOutOfRangeException>(() => table.RemoveAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => table.RemoveAt(-1));

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

        private static EntityTable CreateTable()
        {
            EntityArchetype archetype = EntityArchetype.Create([ComponentType.TypeOf<User>(),
                                                                ComponentType.TypeOf<Position2D>(),
                                                                ComponentType.TypeOf<Tag>()]);
            return new EntityTable(archetype, 16);
        }
    }
}
