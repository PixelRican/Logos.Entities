using System;

namespace Monophyll.Entities.Tests
{
    [TestClass]
    public sealed class EntityRegistryTests
    {

        [TestMethod]
        public void AddComponentTest()
        {
            EntityRegistry registry = new EntityRegistry();
            ReadOnlySpan<ComponentType> componentTypes =
            [
                ComponentType.TypeOf<User>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>()
            ];
            Entity entity = registry.CreateEntity();

            Assert.ThrowsException<ArgumentException>(
                () => registry.AddComponent(new Entity(-1, -1), null!));
            registry.TryGetTable(entity, out EntityTable? table);

            foreach (ComponentType componentType in componentTypes)
            {
                Assert.IsTrue(registry.AddComponent(entity, componentType));
                Assert.IsTrue(registry.ContainsComponent(entity, componentType));
                Assert.IsTrue(registry.GetTables(table!.Archetype).IsEmpty);
                Assert.IsTrue(registry.TryGetTable(entity, out table));
                Assert.IsTrue(componentTypes.StartsWith(table.Archetype.ComponentTypes));
            }
        }

        [TestMethod]
        public void CreateEntityTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.CreateEntity();

                Assert.AreEqual(new Entity(i, 0), entity);
                Assert.IsTrue(registry.ContainsEntity(entity));
                Assert.IsTrue(registry.TryGetTable(entity, out EntityTable? table));
                Assert.AreSame(table.Archetype, EntityArchetype.Base);
                Assert.AreEqual(table.GetEntities()[i], entity);
            }

            Assert.AreEqual(10, registry.Count);

            EntityArchetype archetype = registry.GetArchetype([ComponentType.TypeOf<User>()]);

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.CreateEntity(archetype);

                Assert.AreEqual(new Entity(i + 10, 0), entity);
                Assert.IsTrue(registry.ContainsComponent(entity, ComponentType.TypeOf<User>()));
                Assert.IsTrue(registry.TryGetTable(entity, out EntityTable? table));
                Assert.AreSame(table.Archetype, archetype);
                Assert.AreEqual(table.GetEntities()[i], entity);
            }

            Assert.AreEqual(20, registry.Count);
        }

        [TestMethod]
        public void DestroyEntityTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                registry.CreateEntity();
            }

            foreach (Entity entity in registry.GetTables(EntityArchetype.Base)[0].GetEntities())
            {
                Assert.IsTrue(registry.DestroyEntity(entity));
                Assert.IsFalse(registry.ContainsEntity(entity));
                Assert.IsFalse(registry.TryGetTable(entity, out _));
            }

            Assert.AreEqual(0, registry.Count);
            Assert.IsTrue(registry.GetTables(EntityArchetype.Base).IsEmpty);

            for (int i = 9; i >= 0; i--)
            {
                Assert.AreEqual(new Entity(i, 1), registry.CreateEntity());
            }
        }

        [TestMethod]
        public void RemoveComponentTest()
        {
            EntityRegistry registry = new EntityRegistry();
            ReadOnlySpan<ComponentType> componentTypes =
            [
                ComponentType.TypeOf<User>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Tag>()
            ];
            Entity entity = registry.CreateEntity(componentTypes);

            Assert.ThrowsException<ArgumentException>(
                () => registry.RemoveComponent(new Entity(-1, -1), null!));
            registry.TryGetTable(entity, out EntityTable? table);

            foreach (ComponentType componentType in componentTypes)
            {
                Assert.IsTrue(registry.RemoveComponent(entity, componentType));
                Assert.IsFalse(registry.ContainsComponent(entity, componentType));
                Assert.IsTrue(registry.GetTables(table!.Archetype).IsEmpty);
                Assert.IsTrue(registry.TryGetTable(entity, out table));
                Assert.IsTrue(componentTypes.EndsWith(table.Archetype.ComponentTypes));
            }
        }
    }
}
