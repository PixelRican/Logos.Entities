using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryAddRemoveSetComponentTest : IUnitTest
	{
		public void Run()
		{
			EntityRegistry registry = new();
			Entity entity = registry.CreateEntity();
			EntityQueryResults results;
			int index;

			Debug.Assert(registry.TryAddComponent(entity, TypeOf<Position3D>()));

			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Position3D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(registry.RemoveComponent(entity, out Position3D position) && position.Equals(default));

			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == EntityArchetype.Base);
			Debug.Assert(results[index] == entity);
			Debug.Assert(registry.TrySetComponent(entity, new Rotation3D(Quaternion.Identity)));

			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Rotation3D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(registry.RemoveComponent(entity, out Rotation3D rotation) && rotation.Value.Equals(Quaternion.Identity));

			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == EntityArchetype.Base);
			Debug.Assert(results[index] == entity);

			registry.SetComponent(entity, new Position2D(Vector2.One));
			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Position2D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(results.GetComponents<Position2D>()[index].Value.Equals(Vector2.One));

			registry.SetComponent(entity, new Rotation2D(100.0f));
			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Position2D>(), TypeOf<Rotation2D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(results.GetComponents<Position2D>()[index].Value.Equals(Vector2.One));
			Debug.Assert(results.GetComponents<Rotation2D>()[index].Value.Equals(100.0f));

			registry.SetComponent(entity, new Scale2D(new Vector2(20.0f)));
			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(results.GetComponents<Position2D>()[index].Value.Equals(Vector2.One));
			Debug.Assert(results.GetComponents<Rotation2D>()[index].Value.Equals(100.0f));
			Debug.Assert(results.GetComponents<Scale2D>()[index].Value.Equals(new Vector2(20.0f)));

			registry.RemoveComponent(entity, TypeOf<Scale2D>());
			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Position2D>(), TypeOf<Rotation2D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(results.GetComponents<Position2D>()[index].Value.Equals(Vector2.One));
			Debug.Assert(results.GetComponents<Rotation2D>()[index].Value.Equals(100.0f));

			registry.RemoveComponent(entity, TypeOf<Rotation2D>());
			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == registry.CreateEntityArchetype([TypeOf<Position2D>()]));
			Debug.Assert(results[index] == entity);
			Debug.Assert(results.GetComponents<Position2D>()[index].Value.Equals(Vector2.One));

			registry.RemoveComponent(entity, TypeOf<Position2D>());
			(results, index) = registry.LocateEntity(entity);

			Debug.Assert(results.Archetype == EntityArchetype.Base);
			Debug.Assert(results[index] == entity);
		}
	}
}
