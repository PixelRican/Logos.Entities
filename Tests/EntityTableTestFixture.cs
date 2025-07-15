// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityTableTestFixture
    {
        [Test]
        public void AddTest()
        {
            EntityTable table = CreateTable();
            Assert.That(table.CheckAccess());

            for (int i = 1; i <= table.Capacity; i++)
            {
                table.Add(new Entity(i, 0));
                Assert.That(table.Count, Is.EqualTo(i));
            }

            Assert.That(table.IsFull);
            Assert.Throws<InvalidOperationException>(() => table.Add(new Entity()));
        }

        [Test]
        public void ClearTest()
        {
            EntityTable table = CreateTable();
            Assert.That(table.CheckAccess());

            while (!table.IsFull)
            {
                table.Add(new Entity());
            }

            table.Clear();
            Assert.That(table.IsEmpty);
        }

        [Test]
        public void ConstructorTest()
        {
            EntityTable table = new EntityTable(EntityArchetype.Base);

            Assert.That(table.IsEmpty);
            Assert.That(table.CheckAccess());
            Assert.That(8, Is.EqualTo(table.Capacity));
            Assert.Throws<ArgumentNullException>(() => new EntityTable(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EntityTable(EntityArchetype.Base, -1));
        }

        [Test]
        public void RemoveTest()
        {
            EntityTable table = CreateTable();

            Assert.That(table.CheckAccess());
            Assert.That(table.Remove(new Entity()), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => table.RemoveAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => table.RemoveAt(-1));

            for (int i = 1; i <= table.Capacity; i++)
            {
                table.Add(new Entity(i, 0));
            }

            ReadOnlySpan<Entity> entities = table.GetEntities();

            for (int i = table.Count; i > 1; i--)
            {
                table.RemoveAt(0);
                Assert.That(new Entity(i, 0), Is.EqualTo(entities[0]));
            }

            table.RemoveAt(0);
            Assert.That(new Entity(2, 0), Is.EqualTo(entities[0]));
            Assert.That(table.IsEmpty);
        }

        private static EntityTable CreateTable()
        {
            EntityArchetype archetype = EntityArchetype.Create([ComponentType.TypeOf<Name>(),
                                                                ComponentType.TypeOf<Position2D>(),
                                                                ComponentType.TypeOf<Enabled>()]);
            return new EntityTable(archetype, 16);
        }
    }
}
