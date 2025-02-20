using System;
using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkRangedMutationTest : ITestCase
	{
		public void Execute()
		{
			EntityArchetypeChunk chunkA = new EntityArchetypeChunk(EntityArchetype.Create([
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>()], 0));
			EntityArchetypeChunk chunkB = new EntityArchetypeChunk(chunkA.Archetype.CloneWith(ComponentType.TypeOf<Tag>(), 0));
			EntityArchetypeChunk chunkC = new EntityArchetypeChunk(chunkA.Archetype.CloneWith(ComponentType.TypeOf<Matrix3x2>(), 0));

			for (int i = 0; i < 5; i++)
			{
				chunkA.Push(new Entity(i, 0));
				chunkB.Push(new Entity(i, 0));
				chunkC.Push(new Entity(i, 0));
				chunkA.GetComponents<Position2D>()[i] = new Position2D(new Vector2(i * 10.0f));
				chunkA.GetComponents<Rotation2D>()[i] = new Rotation2D(i * MathF.PI / 4.0f);
				chunkA.GetComponents<Scale2D>()[i] = new Scale2D(new Vector2(i + 1.0f));
				chunkC.GetComponents<Matrix3x2>()[i] = Matrix3x2.Identity;
			}

			chunkB.SetRange(0, chunkA, 0, 5);

			Debug.Assert(chunkB.GetEntities().SequenceEqual(chunkA.GetEntities()));
			Debug.Assert(chunkB.GetComponents<Position2D>().SequenceEqual(chunkA.GetComponents<Position2D>()));
			Debug.Assert(chunkB.GetComponents<Rotation2D>().SequenceEqual(chunkA.GetComponents<Rotation2D>()));
			Debug.Assert(chunkB.GetComponents<Scale2D>().SequenceEqual(chunkA.GetComponents<Scale2D>()));

			chunkC.SetRange(0, chunkA, 0, 5);

			Debug.Assert(chunkC.GetEntities().SequenceEqual(chunkA.GetEntities()));
			Debug.Assert(chunkC.GetComponents<Position2D>().SequenceEqual(chunkA.GetComponents<Position2D>()));
			Debug.Assert(chunkC.GetComponents<Rotation2D>().SequenceEqual(chunkA.GetComponents<Rotation2D>()));
			Debug.Assert(chunkC.GetComponents<Scale2D>().SequenceEqual(chunkA.GetComponents<Scale2D>()));
			Debug.Assert(chunkC.GetComponents<Matrix3x2>().Count(new Matrix3x2()) == 5);

			for (int i = 0; i < 5; i++)
			{
				chunkA.Set(i, new Entity(i, -1));
			}

			chunkA.SetRange(0, chunkC, 0, 5);

			Debug.Assert(chunkA.GetEntities().SequenceEqual(chunkC.GetEntities()));
			Debug.Assert(chunkA.GetComponents<Position2D>().SequenceEqual(chunkC.GetComponents<Position2D>()));
			Debug.Assert(chunkA.GetComponents<Rotation2D>().SequenceEqual(chunkC.GetComponents<Rotation2D>()));
			Debug.Assert(chunkA.GetComponents<Scale2D>().SequenceEqual(chunkC.GetComponents<Scale2D>()));
		}
	}
}
