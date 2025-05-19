using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryCreateDestroyEntityTest : ITestCase
	{
		public void Execute()
		{
			EntityRegistry registry = new EntityRegistry();
			ComponentType[] types =
			[
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>()
			];
			EntityArchetype archetype = registry.GetArchetype(types);

			for (int i = 0; i < 25; i++)
			{
				Debug.Assert(registry.CreateEntity(archetype).Equals(new Entity(i * 4, 0)));
				Debug.Assert(registry.CreateEntity(types).Equals(new Entity(i * 4 + 1, 0)));
				Debug.Assert(registry.CreateEntity((IEnumerable<ComponentType>)types).Equals(new Entity(i * 4 + 2, 0)));
				Debug.Assert(registry.CreateEntity((ReadOnlySpan<ComponentType>)types).Equals(new Entity(i * 4 + 3, 0)));
				Debug.Assert(registry.ContainsEntity(new Entity(i * 4, 0)));
				Debug.Assert(registry.ContainsEntity(new Entity(i * 4 + 1, 0)));
				Debug.Assert(registry.ContainsEntity(new Entity(i * 4 + 2, 0)));
				Debug.Assert(registry.ContainsEntity(new Entity(i * 4 + 3, 0)));
				Debug.Assert(registry.TryGetTable(new Entity(i * 4, 0), out EntityTable? chunk));
				Debug.Assert(chunk!.Archetype.Equals(archetype));
				Debug.Assert(registry.TryGetTable(new Entity(i * 4 + 1, 0), out chunk));
				Debug.Assert(chunk!.Archetype.Equals(archetype));
				Debug.Assert(registry.TryGetTable(new Entity(i * 4 + 2, 0), out chunk));
				Debug.Assert(chunk!.Archetype.Equals(archetype));
				Debug.Assert(registry.TryGetTable(new Entity(i * 4 + 3, 0), out chunk));
				Debug.Assert(chunk!.Archetype.Equals(archetype));
			}

			Debug.Assert(registry.Count == 100);

			int count = 0;

			foreach (EntityTable table in registry.UniversalQuery)
			{
				Debug.Assert(table.Count == 100);

				foreach (Entity entity in table.GetEntities())
				{
					Debug.Assert(entity.Equals(new Entity(count++, 0)));
				}
			}

			for (int i = 0; i < 50; i++)
			{
				Debug.Assert(registry.DestroyEntity(new Entity(i, 0)));
				Debug.Assert(!registry.ContainsEntity(new Entity(i, 0)));
			}

			Debug.Assert(registry.Count == 50);

			foreach (EntityTable chunk in registry.UniversalQuery)
			{
				Debug.Assert(chunk.Count == 50);

				foreach (Entity entity in chunk.GetEntities())
				{
					Debug.Assert(entity.Equals(new Entity(--count, 0)));
				}
			}

			for (int i = 49; i >= 0 ; i--)
			{
				Debug.Assert(registry.CreateEntity(archetype).Equals(new Entity(i, 1)));
				Debug.Assert(registry.ContainsEntity(new Entity(i, 1)));
			}

			Debug.Assert(registry.Count == 100);

			foreach (EntityTable table in registry.UniversalQuery)
			{
				ReadOnlySpan<Entity> entities = table.GetEntities();

				Debug.Assert(table.Count == 100);

				for (int i = 0; i < table.Count; i++)
				{
					Debug.Assert(entities[i].Equals(new Entity(99 - i, i < 50 ? 0 : 1)));
				}
			}
		}
	}
}
