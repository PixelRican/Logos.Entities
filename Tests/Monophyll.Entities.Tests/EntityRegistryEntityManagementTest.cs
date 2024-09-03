using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryEntityManagementTest : IUnitTest
	{
		public void Run()
		{
			EntityRegistry registry = new EntityRegistry();
			Entity entity1 = registry.CreateEntity();
			Entity entity2 = registry.CreateEntity();

			Debug.Assert(entity1 == new Entity(0, 0));
			Debug.Assert(entity2 == new Entity(1, 0));
			Debug.Assert(registry.HasEntity(entity1));
			Debug.Assert(registry.HasEntity(entity2));
			Debug.Assert(registry.TryGetEntityArchetypeChunk(entity1, out EntityArchetypeChunk? chunk1));
			Debug.Assert(registry.TryGetEntityArchetypeChunk(entity2, out EntityArchetypeChunk? chunk2));
			Debug.Assert(chunk1 == chunk2);
			Debug.Assert(chunk1[0] == entity1);
			Debug.Assert(chunk2[1] == entity2);
			Debug.Assert(registry.DestroyEntity(entity1));
			Debug.Assert(!registry.HasEntity(entity1));
			Debug.Assert(registry.HasEntity(entity2));
			Debug.Assert(!registry.TryGetEntityArchetypeChunk(entity1, out _));
			Debug.Assert(registry.TryGetEntityArchetypeChunk(entity2, out chunk2));
			Debug.Assert(chunk2[0] == entity2);

			entity1 = registry.CreateEntity();

			Debug.Assert(entity1 == new Entity(0, 1));
			Debug.Assert(registry.TryGetEntityArchetypeChunk(entity1, out chunk1));
			Debug.Assert(chunk1[^1] == entity1);
			Debug.Assert(registry.DestroyEntity(entity2));
			Debug.Assert(registry.DestroyEntity(entity1));

			entity1 = registry.CreateEntity();
			entity2 = registry.CreateEntity();

			Debug.Assert(entity1 == new Entity(0, 2));
			Debug.Assert(entity2 == new Entity(1, 1));
		}
	}
}
