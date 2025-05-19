using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityTableCreationTest : ITestCase
	{
		public void Execute()
		{
			EntityTable table = new EntityTable(EntityArchetype.Create([
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>()]));

			Debug.Assert(table.Capacity == 8);
			Debug.Assert(table.Count == 0);
			Debug.Assert(!table.IsReadOnly);

			try
			{
				table.GetEntities();
				table.GetComponents<Position3D>();
				table.GetComponents<Rotation3D>();
				table.GetComponents<Scale3D>();
				table.GetEntityDataReference();
				table.GetComponentDataReference<Position3D>();
				table.GetComponentDataReference<Rotation3D>();
				table.GetComponentDataReference<Scale3D>();
			}
			catch (Exception e)
			{
				Debug.Fail("An unexpected exception was thrown.", e.Message);
			}

			try
			{
				table.GetComponents<Matrix4x4>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}

			try
			{
				table.GetComponentDataReference<Matrix4x4>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}

			table = new EntityTable(table.Archetype.Add(ComponentType.TypeOf<Tag>()), this, 0);

			Debug.Assert(table.Capacity == 8);
			Debug.Assert(table.Count == 0);
			Debug.Assert(table.IsReadOnly);
			Monitor.Enter(this);
			Debug.Assert(!table.IsReadOnly);
			Monitor.Exit(this);
			Debug.Assert(table.IsReadOnly);

			try
			{
				table.GetEntities();
				table.GetComponents<Position3D>();
				table.GetComponents<Rotation3D>();
				table.GetComponents<Scale3D>();
				table.GetEntityDataReference();
				table.GetComponentDataReference<Position3D>();
				table.GetComponentDataReference<Rotation3D>();
				table.GetComponentDataReference<Scale3D>();
			}
			catch (Exception e)
			{
				Debug.Fail("An unexpected exception was thrown.", e.Message);
			}

			try
			{
				table.GetComponents<Tag>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}

			try
			{
				table.GetComponentDataReference<Tag>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}
		}
	}
}
