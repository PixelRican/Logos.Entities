// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities.Tests
{
    [TestFixture, TestOf(typeof(EntityRegistry))]
    public static class EntityRegistryTestFixture
    {
        [Test]
        public static void AddComponentTest()
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
                ComponentType.TypeOf<Disabled>()
            ];
            Entity entity = registry.Create();

            Assert.Throws<EntityNotFoundException>(() =>
            {
                registry.AddComponent(new Entity(-1, -1), ComponentType.TypeOf<Name>());
            });

            for (int i = 0; i < componentTypes.Length; i++)
            {
                ComponentType componentType = componentTypes[i];

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(registry.AddComponent(entity, componentType), Is.True);
                    Assert.That(registry.HasComponent(entity, componentType), Is.True);
                    Assert.That(registry.Find(entity, out _).Archetype.ComponentTypes
                        .SequenceEqual(componentTypes.Slice(0, i + 1)), Is.True);
                }
            }
        }

        [Test]
        public static void CreateTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.Create();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(new Entity(i, 0), Is.EqualTo(entity));
                    Assert.That(registry.Contains(entity));
                    Assert.That(registry.Find(entity, out _).Archetype, Is.SameAs(EntityArchetype.Base));
                    Assert.That(registry.Find(entity, out _).GetEntities()[i], Is.EqualTo(entity));
                }
            }

            Assert.That(registry.Count, Is.EqualTo(10));

            EntityArchetype archetype = EntityArchetype.Create([ComponentType.TypeOf<Name>()]);

            for (int i = 0; i < 10; i++)
            {
                Entity entity = registry.Create(archetype);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(new Entity(i + 10, 0), Is.EqualTo(entity));
                    Assert.That(registry.HasComponent(entity, ComponentType.TypeOf<Name>()));
                    Assert.That(registry.Find(entity, out _).Archetype, Is.SameAs(archetype));
                    Assert.That(registry.Find(entity, out _).GetEntities()[i], Is.EqualTo(entity));
                }
            }

            Assert.That(registry.Count, Is.EqualTo(20));
        }

        [Test]
        public static void DeleteTest()
        {
            EntityRegistry registry = new EntityRegistry();

            for (int i = 0; i < 10; i++)
            {
                registry.Create();
            }

            EntityTable table = registry.Find(new Entity(), out _);
            ReadOnlySpan<Entity> entities = table.GetEntities();
            int count = table.Count;

            for (int i = 0; i < count; i++)
            {
                Entity entity = entities[i];

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(registry.Delete(entity));
                    Assert.That(registry.Contains(entity), Is.False);
                }
            }

            Assert.That(registry.Count, Is.Zero);

            for (int i = 9; i >= 0; i--)
            {
                Assert.That(new Entity(i, 1), Is.EqualTo(registry.Create()));
            }
        }

        [Test]
        public static void RemoveComponentTest()
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
                ComponentType.TypeOf<Disabled>()
            ];
            EntityArchetype archetype = EntityArchetype.Create(componentTypes);
            Entity entity = registry.Create(archetype);

            Assert.Throws<EntityNotFoundException>(() =>
            {
                registry.RemoveComponent(new Entity(-1, -1), ComponentType.TypeOf<Name>());
            });

            for (int i = 0; i < componentTypes.Length; i++)
            {
                ComponentType componentType = componentTypes[i];

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(registry.RemoveComponent(entity, componentType));
                    Assert.That(registry.HasComponent(entity, componentType), Is.False);
                    Assert.That(registry.Find(entity, out _).Archetype.ComponentTypes
                        .SequenceEqual(componentTypes.Slice(i + 1)), Is.True);
                }
            }
        }
    }
}
