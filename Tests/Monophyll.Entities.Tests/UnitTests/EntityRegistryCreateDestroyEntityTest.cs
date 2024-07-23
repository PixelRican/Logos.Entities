using System;
using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryCreateDestroyEntityTest : IUnitTest
	{
		public void Run()
		{
			EntityRegistry registry = new();
			registry.EntityCreated += OnEntityCreated;
			registry.EntityDestroyed += OnEntityDestroyed;

			Span<Entity> entities =
			[
				registry.CreateEntity(),
				registry.CreateEntity(),
				registry.CreateEntity(),
				registry.CreateEntity(),
				registry.CreateEntity()
			];

			for (int i = 0; i < entities.Length; i++)
			{
				ref Entity entity = ref entities[i];
				Debug.Assert(entity.Id == i);
				Debug.Assert(entity.Version == 0);
				Debug.Assert(registry.DestroyEntity(entity));

				entity = registry.CreateEntity();
				Debug.Assert(entity.Id == i);
				Debug.Assert(entity.Version == 1);
			}

			registry.EntityCreated -= OnEntityCreated;
			registry.EntityDestroyed -= OnEntityDestroyed;

			void OnEntityCreated(object? sender, Entity args)
			{
				Debug.Assert(registry.IsEntityAlive(args));
			}

			void OnEntityDestroyed(object? sender, Entity args)
			{
				Debug.Assert(!registry.IsEntityAlive(args));
			}
		}
	}
}
