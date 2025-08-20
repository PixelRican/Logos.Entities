// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities.Tests
{
    [TestFixture]
    public static class EntityTableTestFixture
    {
        [Test]
        public static void CreateRowTest()
        {
            EntityTable table = CreateTestTable();
            ReadOnlySpan<Entity> entities = table.GetEntities();
            Span<Name> names = table.GetComponents<Name>();
            Span<Position2D> positions = table.GetComponents<Position2D>();
            Entity entity = new Entity(-1, -1);
            Name name = new Name()
            {
                Value = "KEEP ME"
            };
            Position2D position = new Position2D()
            {
                X = 1,
                Y = 2
            };

            names.Fill(name);
            positions.Fill(position);

            Assert.That(table.IsEmpty, Is.True);

            for (int i = 0; i < table.Capacity; i++)
            {
                table.CreateRow(entity);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(table.Count, Is.EqualTo(i + 1));
                    Assert.That(entities[i], Is.EqualTo(entity));
                    Assert.That(names[i], Is.EqualTo(name));
                    Assert.That(positions[i], Is.Default);
                }
            }

            Assert.That(table.IsFull, Is.True);
            Assert.Throws<InvalidOperationException>(() =>
            {
                table.CreateRow(entity);
            });
        }

        [Test]
        public static void ClearRowsTest()
        {
            EntityTable table = CreateTestTable();
            ReadOnlySpan<Entity> entities = table.GetEntities();
            Span<Name> names = table.GetComponents<Name>();
            Span<Position2D> positions = table.GetComponents<Position2D>();
            Entity entity = new Entity(-1, -1);
            Name name = new Name()
            {
                Value = "FREE ME"
            };
            Position2D position = new Position2D()
            {
                X = -1,
                Y = -1
            };

            while (!table.IsFull)
            {
                table.CreateRow(entity);
            }

            names.Fill(name);
            positions.Fill(position);
            table.ClearRows();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(entities.Count(entity), Is.EqualTo(table.Capacity));
                Assert.That(names.Count(name), Is.Zero);
                Assert.That(positions.Count(position), Is.EqualTo(table.Capacity));
            }
        }

        [Test]
        public static void ConstructorExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new EntityTable(null!, 0);
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new EntityTable(EntityArchetype.Base, -1);
            });
        }

        [TestCaseSource(typeof(EntityTableTestCaseSource), nameof(EntityTableTestCaseSource.ConstructorTestCases))]
        public static void ConstructorTest(EntityArchetype archetype)
        {
            EntityTable table = new EntityTable(archetype, 1);

            AssertHelper<Name>();
            AssertHelper<Disabled>();
            AssertHelper<Position2D>();
            AssertHelper<Position3D>();
            AssertHelper<Rotation2D>();
            AssertHelper<Rotation3D>();
            AssertHelper<Scale2D>();
            AssertHelper<Scale3D>();

            void AssertHelper<T>()
            {
                if (table.TryGetComponents(out Span<T> components))
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(ComponentType.TypeOf<T>().Category,
                            Is.Not.EqualTo(ComponentTypeCategory.Tag));
                        Assert.That(archetype.Contains(ComponentType.TypeOf<T>()), Is.True);
                        Assert.That(components.Length, Is.EqualTo(table.Capacity));
                    }
                }
                else
                {
                    if (ComponentType.TypeOf<T>().Category != ComponentTypeCategory.Tag)
                    {
                        Assert.That(archetype.Contains(ComponentType.TypeOf<T>()), Is.False);
                    }

                    Assert.Throws<ComponentNotFoundException>(() =>
                    {
                        table.GetComponents<T>();
                    });
                }
            }
        }

        [Test]
        public static void DeleteRowTest()
        {
            EntityTable table = CreateTestTable();
            ReadOnlySpan<Entity> entities = table.GetEntities();
            Span<Name> names = table.GetComponents<Name>();
            Span<Position2D> positions = table.GetComponents<Position2D>();
            Name name = new Name()
            {
                Value = "FREE ME"
            };
            Position2D position = new Position2D()
            {
                X = 1,
                Y = 2
            };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                table.DeleteRow(0);
            });

            for (int i = 0; i < table.Capacity; i++)
            {
                table.CreateRow(new Entity(i, 0));
            }

            names.Fill(name);
            positions.Fill(position);
            table.DeleteRow(0);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(table.Count, Is.EqualTo(table.Capacity - 1));
                Assert.That(entities[0], Is.EqualTo(new Entity(7, 0)));
                Assert.That(entities[7], Is.EqualTo(new Entity(7, 0)));
                Assert.That(positions[7], Is.EqualTo(position));
                Assert.That(names[7], Is.Default);
            }

            for (int i = 6; i >= 0; i--)
            {
                table.DeleteRow(i);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(table.IsEmpty, Is.True);
                Assert.That(positions.Count(position), Is.EqualTo(table.Capacity));
                Assert.That(names.Count(name), Is.Zero);
            }
        }

        private static EntityTable CreateTestTable()
        {
            return new EntityTable(EntityArchetype.Create(new ComponentType[]
            {
                ComponentType.TypeOf<Name>(),
                ComponentType.TypeOf<Position2D>(),
                ComponentType.TypeOf<Disabled>()
            }), 8);
        }

        [Test]
        public static void InvalidRowOperationTest()
        {
            EntityTable table = new EntityTable(EntityArchetype.Base, new EntityRegistry(), 0);

            Assert.Throws<InvalidOperationException>(() =>
            {
                table.CreateRow(default);
            });
            Assert.Throws<InvalidOperationException>(() =>
            {
                table.ImportRow(null!, 0);
            });
            Assert.Throws<InvalidOperationException>(table.ClearRows);
            Assert.Throws<InvalidOperationException>(() =>
            {
                table.DeleteRow(0);
            });
        }
    }
}
