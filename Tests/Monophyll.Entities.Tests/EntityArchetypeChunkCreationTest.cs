using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkCreationTest : ITestCase
	{
		public void Execute()
		{
			EntityArchetypeChunk chunk = new EntityArchetypeChunk(EntityArchetype.Create([
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>()], 0));

			Debug.Assert(chunk.Capacity == 8);
			Debug.Assert(chunk.Count == 0);
			Debug.Assert(chunk.IsModifiable);

			try
			{
				chunk.GetEntities();
				chunk.GetComponents<Position3D>();
				chunk.GetComponents<Rotation3D>();
				chunk.GetComponents<Scale3D>();
				chunk.GetEntityDataReference();
				chunk.GetComponentDataReference<Position3D>();
				chunk.GetComponentDataReference<Rotation3D>();
				chunk.GetComponentDataReference<Scale3D>();
			}
			catch (Exception e)
			{
				Debug.Fail("An unexpected exception was thrown.", e.Message);
			}

			try
			{
				chunk.GetComponents<Matrix4x4>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}

			try
			{
				chunk.GetComponentDataReference<Matrix4x4>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}

			chunk = new EntityArchetypeChunk(chunk.Archetype.CloneWith(ComponentType.TypeOf<Tag>(), 0), this, 0);

			Debug.Assert(chunk.Capacity == 8);
			Debug.Assert(chunk.Count == 0);
			Debug.Assert(!chunk.IsModifiable);
			Monitor.Enter(this);
			Debug.Assert(chunk.IsModifiable);
			Monitor.Exit(this);
			Debug.Assert(!chunk.IsModifiable);

			try
			{
				chunk.GetEntities();
				chunk.GetComponents<Position3D>();
				chunk.GetComponents<Rotation3D>();
				chunk.GetComponents<Scale3D>();
				chunk.GetEntityDataReference();
				chunk.GetComponentDataReference<Position3D>();
				chunk.GetComponentDataReference<Rotation3D>();
				chunk.GetComponentDataReference<Scale3D>();
			}
			catch (Exception e)
			{
				Debug.Fail("An unexpected exception was thrown.", e.Message);
			}

			try
			{
				chunk.GetComponents<Tag>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}

			try
			{
				chunk.GetComponentDataReference<Tag>();
				Debug.Fail("An expected exception was not thrown.");
			}
			catch (Exception e)
			{
				Debug.Assert(e is ArgumentException, "An unexpected exception was thrown.", e.Message);
			}
		}
	}
}
