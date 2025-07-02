// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    [TestFixture]
    public sealed class EntityRegistryTests
    {
        [Test]
        public void AddComponentTest()
        {
            EntityRegistry registry = new EntityRegistry();
            ReadOnlySpan<ComponentType> componentTypes =
            [
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Enabled>()
            ];
            Entity entity = registry.CreateEntity();

            Assert.Throws<ArgumentException>(
                () => registry.AddComponent(new Entity(-1, -1), null!));

            foreach (ComponentType componentType in componentTypes)
            {
                Assert.IsTrue(registry.AddComponent(entity, componentType));
                Assert.IsTrue(registry.HasComponent(entity, componentType));
                Assert.IsTrue(componentTypes.StartsWith(
                    registry.FindEntity(entity).Archetype.ComponentTypes));
            }
        }

        [Test]
        public void CreateEntityTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.CreateEntity();

                Assert.AreEqual(new Entity(i, 0), entity);
                Assert.IsTrue(registry.ContainsEntity(entity));
                Assert.AreSame(registry.FindEntity(entity).Archetype, EntityArchetype.Base);
                Assert.AreEqual(registry.FindEntity(entity).GetEntities()[i], entity);
            }

            Assert.AreEqual(10, registry.Count);

            EntityArchetype archetype = registry.CreateArchetype([ComponentType.TypeOf<Name>()]);

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.CreateEntity(archetype);

                Assert.AreEqual(new Entity(i + 10, 0), entity);
                Assert.IsTrue(registry.HasComponent(entity, ComponentType.TypeOf<Name>()));
                Assert.AreSame(registry.FindEntity(entity).Archetype, archetype);
                Assert.AreEqual(registry.FindEntity(entity).GetEntities()[i], entity);
            }

            Assert.AreEqual(20, registry.Count);
        }

        [Test]
        public void DestroyEntityTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                registry.CreateEntity();
            }

            foreach (Entity entity in registry.FindEntity(new Entity()).GetEntities())
            {
                Assert.IsTrue(registry.DestroyEntity(entity));
                Assert.IsFalse(registry.ContainsEntity(entity));
            }

            Assert.AreEqual(0, registry.Count);

            for (int i = 9; i >= 0; i--)
            {
                Assert.AreEqual(new Entity(i, 1), registry.CreateEntity());
            }
        }

        [Test]
        public void RemoveComponentTest()
        {
            EntityRegistry registry = new EntityRegistry();
            ReadOnlySpan<ComponentType> componentTypes =
            [
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation2D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale2D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Enabled>()
            ];
            Entity entity = registry.CreateEntity(componentTypes);

            Assert.Throws<ArgumentException>(
                () => registry.RemoveComponent(new Entity(-1, -1), null!));

            foreach (ComponentType componentType in componentTypes)
            {
                Assert.IsTrue(registry.RemoveComponent(entity, componentType));
                Assert.IsFalse(registry.HasComponent(entity, componentType));
                Assert.IsTrue(componentTypes.EndsWith(registry.FindEntity(entity).Archetype.ComponentTypes));
            }
        }
    }
}
