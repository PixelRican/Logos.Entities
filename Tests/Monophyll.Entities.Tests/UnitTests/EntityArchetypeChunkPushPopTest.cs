using System;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkPushPopTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetypeChunk chunk = new(new EntityArchetype([
				TypeOf<Position2D>(),
				TypeOf<Rotation2D>(),
				TypeOf<Scale2D>(),
				TypeOf<Matrix3x2>(),
				TypeOf<Tag>()
			]));
			ReadOnlySpan<Entity> entities = chunk.GetEntities();
			Span<Position2D> positions = chunk.GetComponents<Position2D>();
			Span<Rotation2D> rotations = chunk.GetComponents<Rotation2D>();
			Span<Scale2D> scales = chunk.GetComponents<Scale2D>();
			Span<Matrix3x2> matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(chunk.ByteSize == chunk.Archetype.ChunkByteSize);
			Debug.Assert(entities.Length == 0);
			Debug.Assert(positions.Length == 0);
			Debug.Assert(rotations.Length == 0);
			Debug.Assert(scales.Length == 0);
			Debug.Assert(matrices.Length == 0);

			try
			{
				Span<Tag> tags = chunk.GetComponents<Tag>();
				Debug.Fail("EntityArchetypeChunks should not allow spans of tags to be created.");
			}
			catch (ArgumentException)
			{
			}

			try
			{
				Span<Rotation3D> quaternions = chunk.GetComponents<Rotation3D>();
				Debug.Fail("EntityArchetypeChunks should throw an exception for components it does not store.");
			}
			catch (ArgumentException)
			{
			}

			chunk.Push(new Entity(1, 0));
			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(entities.Length == 1);
			Debug.Assert(entities[0].Equals(new Entity(1, 0)));
			Debug.Assert(positions.Length == 1);
			Debug.Assert(rotations.Length == 1);
			Debug.Assert(scales.Length == 1);
			Debug.Assert(matrices.Length == 1);

			positions[0].Value = new Vector2(100, 120);
			rotations[0].Value = 90.0f;
			scales[0].Value = new Vector2(1.0f);
			matrices[0] = Matrix3x2.CreateTranslation(positions[0].Value) *
						  Matrix3x2.CreateRotation(rotations[0].Value) *
						  Matrix3x2.CreateScale(scales[0].Value);

			Debug.Assert(entities[0].Equals(chunk.Pop()));

			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(entities.Length == 0);
			Debug.Assert(positions.Length == 0);
			Debug.Assert(rotations.Length == 0);
			Debug.Assert(scales.Length == 0);
			Debug.Assert(matrices.Length == 0);

			chunk.Push(new Entity(1, 1));

			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(entities.Length == 1);
			Debug.Assert(entities[0].Equals(new Entity(1, 1)));

			Debug.Assert(positions.Length == 1);
			Debug.Assert(positions[0].Value.Equals(Vector2.Zero));

			Debug.Assert(rotations[0].Value == 0.0f);
			Debug.Assert(rotations.Length == 1);

			Debug.Assert(scales.Length == 1);
			Debug.Assert(scales[0].Value.Equals(Vector2.Zero));

			Debug.Assert(matrices.Length == 1);
			Debug.Assert(matrices[0].Equals(default));

			while (chunk.TryPush(new Entity(-1, 0)))
			{
			}

			Debug.Assert(chunk.Count == chunk.Archetype.ChunkCapacity);

			while (chunk.TryPop(out _))
			{
			}

			Debug.Assert(chunk.Count == 0);
		}
	}
}
