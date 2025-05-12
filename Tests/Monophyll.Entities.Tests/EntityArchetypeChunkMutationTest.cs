using System;
using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkMutationTest : ITestCase
	{
		public void Execute()
		{
			EntityArchetypeChunk chunk = new EntityArchetypeChunk(EntityArchetype.Create([
				ComponentType.TypeOf<object>(),
				ComponentType.TypeOf<Matrix4x4>(),
				ComponentType.TypeOf<Tag>()]));

			chunk.Add(new Entity(1, 0));

			ReadOnlySpan<Entity> entities = chunk.GetEntities();
			Span<Matrix4x4> matrices = chunk.GetComponents<Matrix4x4>();
			Span<object> objects = chunk.GetComponents<object>();

			Debug.Assert(chunk.Version == 1);
			Debug.Assert(chunk.Count == 1);
			Debug.Assert(entities.Length == 1);
			Debug.Assert(matrices.Length == 1);
			Debug.Assert(objects.Length == 1);
			Debug.Assert(entities[0].Equals(new Entity(1, 0)));
			Debug.Assert(matrices[0].Equals(default));
			Debug.Assert(objects[0] == null);

			objects[0] = this;
			matrices[0] = Matrix4x4.Identity;
			chunk.RemoveAt(0);

			Debug.Assert(matrices[0].Equals(Matrix4x4.Identity));
			Debug.Assert(objects[0] == null);
			Debug.Assert(chunk.Version == 2);
			Debug.Assert(chunk.Count == 0);
			Debug.Assert(chunk.GetEntities().Length == 0);
			Debug.Assert(chunk.GetComponents<Matrix4x4>().Length == 0);
			Debug.Assert(chunk.GetComponents<object>().Length == 0);

			objects[0] = this;
			chunk.Add(new Entity(1, 1));

			Debug.Assert(entities[0].Equals(new Entity(1, 1)));
			Debug.Assert(matrices[0].Equals(default));
			Debug.Assert(objects[0] == this);
			Debug.Assert(chunk.Version == 3);
			Debug.Assert(chunk.Count == 1);
			Debug.Assert(chunk.GetEntities().Length == 1);
			Debug.Assert(chunk.GetComponents<Matrix4x4>().Length == 1);
			Debug.Assert(chunk.GetComponents<object>().Length == 1);

			matrices[0] = Matrix4x4.Identity;
			chunk.Set(0, new Entity(1, 2));

			Debug.Assert(entities[0].Equals(new Entity(1, 2)));
			Debug.Assert(matrices[0].Equals(default));
			Debug.Assert(objects[0] == null);
			Debug.Assert(chunk.Version == 4);
			Debug.Assert(chunk.Count == 1);
			Debug.Assert(chunk.GetEntities().Length == 1);
			Debug.Assert(chunk.GetComponents<Matrix4x4>().Length == 1);
			Debug.Assert(chunk.GetComponents<object>().Length == 1);
		}
	}
}
