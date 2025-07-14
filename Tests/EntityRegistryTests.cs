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
                Assert.That(registry.AddComponent(entity, componentType));
                Assert.That(registry.HasComponent(entity, componentType));
                Assert.That(componentTypes.StartsWith(
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

                Assert.That(new Entity(i, 0), Is.EqualTo(entity));
                Assert.That(registry.ContainsEntity(entity));
                Assert.That(registry.FindEntity(entity).Archetype, Is.SameAs(EntityArchetype.Base));
                Assert.That(registry.FindEntity(entity).GetEntities()[i], Is.EqualTo(entity));
            }

            Assert.That(10, Is.EqualTo(registry.Count));

            EntityArchetype archetype = registry.CreateArchetype([ComponentType.TypeOf<Name>()]);

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.CreateEntity(archetype);

                Assert.That(new Entity(i + 10, 0), Is.EqualTo(entity));
                Assert.That(registry.HasComponent(entity, ComponentType.TypeOf<Name>()));
                Assert.That(registry.FindEntity(entity).Archetype, Is.SameAs(archetype));
                Assert.That(registry.FindEntity(entity).GetEntities()[i], Is.EqualTo(entity));
            }

            Assert.That(20, Is.EqualTo(registry.Count));
        }

        [Test]
        public void DestroyEntityTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                registry.CreateEntity();
            }

            EntityTable table = registry.FindEntity(new Entity());
            ReadOnlySpan<Entity> entities = table.GetEntities();
            int count = table.Count;

            for (int i = 0; i < count; i++)
            {
                Entity entity = entities[i];
                Assert.That(registry.DestroyEntity(entity));
                Assert.That(registry.ContainsEntity(entity), Is.False);
            }

            Assert.That(0, Is.EqualTo(registry.Count));

            for (int i = 9; i >= 0; i--)
            {
                Assert.That(new Entity(i, 1), Is.EqualTo(registry.CreateEntity()));
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
                Assert.That(registry.RemoveComponent(entity, componentType));
                Assert.That(registry.HasComponent(entity, componentType), Is.False);
                Assert.That(componentTypes.EndsWith(registry.FindEntity(entity).Archetype.ComponentTypes));
            }
        }
    }
}
