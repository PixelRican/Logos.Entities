using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryAddRemoveComponentTest : ITestCase
	{
		public void Execute()
		{
			EntityRegistry registry = new EntityRegistry();
			Entity entity = registry.CreateEntity();
			EntityArchetypeChunk currentChunk;
			EntityArchetypeChunk previousChunk;

			Debug.Assert(registry.TryGetChunk(entity, out currentChunk!));
			Debug.Assert(currentChunk.Archetype.ComponentTypes.Length == 0);

			previousChunk = currentChunk;
			registry.AddComponent<Position2D>(entity);

			Debug.Assert(registry.TryGetChunk(entity, out currentChunk!));
			Debug.Assert(currentChunk != previousChunk);
			Debug.Assert(currentChunk.Count == 1);
			Debug.Assert(previousChunk.IsEmpty);

			previousChunk = currentChunk;
			registry.RemoveComponent<Position2D>(entity);

			Debug.Assert(registry.TryGetChunk(entity, out currentChunk!));
			Debug.Assert(currentChunk != previousChunk);
			Debug.Assert(currentChunk.Count == 1);
			Debug.Assert(previousChunk.IsEmpty);

		}
	}
}
