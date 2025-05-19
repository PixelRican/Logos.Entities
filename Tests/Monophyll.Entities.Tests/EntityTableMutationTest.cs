using System;
using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityTableMutationTest : ITestCase
	{
		public void Execute()
		{
			EntityTable table = new EntityTable(EntityArchetype.Create([
				ComponentType.TypeOf<object>(),
				ComponentType.TypeOf<Matrix4x4>(),
				ComponentType.TypeOf<Tag>()]));

			table.Add(new Entity(1, 0));

			ReadOnlySpan<Entity> entities = table.GetEntities();
			Span<Matrix4x4> matrices = table.GetComponents<Matrix4x4>();
			Span<object> objects = table.GetComponents<object>();

			Debug.Assert(table.Version == 1);
			Debug.Assert(table.Count == 1);
			Debug.Assert(entities.Length == 1);
			Debug.Assert(matrices.Length == 1);
			Debug.Assert(objects.Length == 1);
			Debug.Assert(entities[0].Equals(new Entity(1, 0)));
			Debug.Assert(matrices[0].Equals(default));
			Debug.Assert(objects[0] == null);

			objects[0] = this;
			matrices[0] = Matrix4x4.Identity;
			table.RemoveAt(0);

			Debug.Assert(matrices[0].Equals(Matrix4x4.Identity));
			Debug.Assert(objects[0] == null);
			Debug.Assert(table.Version == 2);
			Debug.Assert(table.Count == 0);
			Debug.Assert(table.GetEntities().Length == 0);
			Debug.Assert(table.GetComponents<Matrix4x4>().Length == 0);
			Debug.Assert(table.GetComponents<object>().Length == 0);

			objects[0] = this;
			table.Add(new Entity(1, 1));

			Debug.Assert(entities[0].Equals(new Entity(1, 1)));
			Debug.Assert(matrices[0].Equals(default));
			Debug.Assert(objects[0] == this);
			Debug.Assert(table.Version == 3);
			Debug.Assert(table.Count == 1);
			Debug.Assert(table.GetEntities().Length == 1);
			Debug.Assert(table.GetComponents<Matrix4x4>().Length == 1);
			Debug.Assert(table.GetComponents<object>().Length == 1);

			matrices[0] = Matrix4x4.Identity;
			table.Set(0, new Entity(1, 2));

			Debug.Assert(entities[0].Equals(new Entity(1, 2)));
			Debug.Assert(matrices[0].Equals(default));
			Debug.Assert(objects[0] == null);
			Debug.Assert(table.Version == 4);
			Debug.Assert(table.Count == 1);
			Debug.Assert(table.GetEntities().Length == 1);
			Debug.Assert(table.GetComponents<Matrix4x4>().Length == 1);
			Debug.Assert(table.GetComponents<object>().Length == 1);
		}
	}
}
