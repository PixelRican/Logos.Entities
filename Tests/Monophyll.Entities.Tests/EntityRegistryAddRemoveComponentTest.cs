using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryAddRemoveComponentTest : ITestCase
	{
		public void Execute()
		{
			EntityRegistry registry = new EntityRegistry();
			Entity entity = registry.CreateEntity();
			EntityTable current;
			EntityTable previous;

			Debug.Assert(registry.TryGetTable(entity, out current!));
			Debug.Assert(current.Archetype.ComponentTypes.Length == 0);

			previous = current;
			registry.AddComponent<Position2D>(entity);

			Debug.Assert(registry.TryGetTable(entity, out current!));
			Debug.Assert(current != previous);
			Debug.Assert(current.Count == 1);
			Debug.Assert(previous.IsEmpty);

			previous = current;
			registry.RemoveComponent<Position2D>(entity);

			Debug.Assert(registry.TryGetTable(entity, out current!));
			Debug.Assert(current != previous);
			Debug.Assert(current.Count == 1);
			Debug.Assert(previous.IsEmpty);
		}
	}
}
