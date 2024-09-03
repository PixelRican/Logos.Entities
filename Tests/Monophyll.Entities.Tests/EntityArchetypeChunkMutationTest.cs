using System;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkMutationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype archetype = EntityArchetype.Create(
				[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()]);
			EntityArchetypeChunk chunk = new EntityArchetypeChunk(archetype)
			{
				new Entity(0, 0),
				new Entity(1, 0),
				new Entity(2, 0),
				new Entity(3, 0),
				new Entity(4, 0),
			};

			ReadOnlySpan<Entity> entities = chunk.GetEntities();
			Span<Position2D> positions = chunk.GetComponents<Position2D>();
			Span<Rotation2D> rotations = chunk.GetComponents<Rotation2D>();
			Span<Scale2D> scales = chunk.GetComponents<Scale2D>();
			Span<Matrix3x2> matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(chunk.Count == 5);
			Debug.Assert(entities.Length == 5);
			Debug.Assert(positions.Length == 5);
			Debug.Assert(rotations.Length == 5);
			Debug.Assert(scales.Length == 5);
			Debug.Assert(matrices.Length == 5);

			for (int i = 0; i < entities.Length; i++)
			{
				ref readonly Entity entity = ref entities[i];
				ref Vector2 position = ref positions[i].Value;
				ref float rotation = ref rotations[i].Value;
				ref Vector2 scale = ref scales[i].Value;
				ref Matrix3x2 matrix = ref matrices[i];

				Debug.Assert(entity == chunk[i]);
				Debug.Assert(position == Vector2.Zero);
				Debug.Assert(rotation == 0.0f);
				Debug.Assert(scale == Vector2.Zero);
				Debug.Assert(matrix == default);

				position = new Vector2((i + 1.0f) * 50.0f);
				rotation = (i + 1.0f) * 15.0f;
				scale = new Vector2(i + 1.0f);
				matrix = Matrix3x2.CreateTranslation(positions[i].Value) *
						 Matrix3x2.CreateRotation(rotations[i].Value) *
						 Matrix3x2.CreateScale(scales[i].Value);
			}

			chunk.RemoveAt(chunk.Count - 1);
			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(chunk.Count == 4);
			Debug.Assert(entities.Length == 4);
			Debug.Assert(positions.Length == 4);
			Debug.Assert(rotations.Length == 4);
			Debug.Assert(scales.Length == 4);
			Debug.Assert(matrices.Length == 4);

			chunk.Add(new Entity(4, 1));
			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(entities[4] == new Entity(4, 1));
			Debug.Assert(positions[4] == default);
			Debug.Assert(rotations[4] == default);
			Debug.Assert(scales[4] == default);
			Debug.Assert(matrices[4] == default);

			chunk.Remove(new Entity(0, 0));
			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(chunk.Count == 4);
			Debug.Assert(entities.Length == 4);
			Debug.Assert(positions.Length == 4);
			Debug.Assert(rotations.Length == 4);
			Debug.Assert(scales.Length == 4);
			Debug.Assert(matrices.Length == 4);

			Debug.Assert(entities[0] == new Entity(1, 0));
			Debug.Assert(positions[0].Value == new Vector2(100.0f));
			Debug.Assert(rotations[0].Value == 30.0f);
			Debug.Assert(scales[0].Value == new Vector2(2.0f));

			Debug.Assert(entities[1] == new Entity(2, 0));
			Debug.Assert(positions[1].Value == new Vector2(150.0f));
			Debug.Assert(rotations[1].Value == 45.0f);
			Debug.Assert(scales[1].Value == new Vector2(3.0f));

			Debug.Assert(entities[2] == new Entity(3, 0));
			Debug.Assert(positions[2].Value == new Vector2(200.0f));
			Debug.Assert(rotations[2].Value == 60.0f);
			Debug.Assert(scales[2].Value == new Vector2(4.0f));

			Debug.Assert(entities[3] == new Entity(4, 1));
			Debug.Assert(positions[3] == default);
			Debug.Assert(rotations[3] == default);
			Debug.Assert(scales[3] == default);

			chunk.Insert(0, new Entity(0, 1));
			entities = chunk.GetEntities();
			positions = chunk.GetComponents<Position2D>();
			rotations = chunk.GetComponents<Rotation2D>();
			scales = chunk.GetComponents<Scale2D>();
			matrices = chunk.GetComponents<Matrix3x2>();

			Debug.Assert(chunk.Count == 5);
			Debug.Assert(entities.Length == 5);
			Debug.Assert(positions.Length == 5);
			Debug.Assert(rotations.Length == 5);
			Debug.Assert(scales.Length == 5);
			Debug.Assert(matrices.Length == 5);

			Debug.Assert(entities[0] == new Entity(0, 1));
			Debug.Assert(positions[0] == default);
			Debug.Assert(rotations[0] == default);
			Debug.Assert(scales[0] == default);

			Debug.Assert(entities[1] == new Entity(1, 0));
			Debug.Assert(positions[1].Value == new Vector2(100.0f));
			Debug.Assert(rotations[1].Value == 30.0f);
			Debug.Assert(scales[1].Value == new Vector2(2.0f));

			Debug.Assert(entities[2] == new Entity(2, 0));
			Debug.Assert(positions[2].Value == new Vector2(150.0f));
			Debug.Assert(rotations[2].Value == 45.0f);
			Debug.Assert(scales[2].Value == new Vector2(3.0f));

			Debug.Assert(entities[3] == new Entity(3, 0));
			Debug.Assert(positions[3].Value == new Vector2(200.0f));
			Debug.Assert(rotations[3].Value == 60.0f);
			Debug.Assert(scales[3].Value == new Vector2(4.0f));

			Debug.Assert(entities[4] == new Entity(4, 1));
			Debug.Assert(positions[4] == default);
			Debug.Assert(rotations[4] == default);
			Debug.Assert(scales[4] == default);

			try
			{
				chunk.Insert(6, new Entity(5, 0));
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunk.Insert(-1, new Entity(5, 0));
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunk.RemoveAt(5);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunk.RemoveAt(-1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				while (true)
				{
					chunk.Add(new Entity(-1, -1));
				}
			}
			catch (InvalidOperationException)
			{
				Debug.Assert(chunk.IsFull);
			}

			chunk.Clear();
			Debug.Assert(chunk.Count == 0);
		}
	}
}
