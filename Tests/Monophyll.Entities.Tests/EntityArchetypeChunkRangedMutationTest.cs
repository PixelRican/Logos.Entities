using System;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkRangedMutationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetypeChunk chunkWithoutComponents = [];
			EntityArchetypeChunk chunkWithoutMatrices = new(EntityArchetype.Create(
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>()]));
			EntityArchetypeChunk chunkWithMatrices = new(
				chunkWithoutMatrices.Archetype.Add(TypeOf<Matrix4x4>()));
			ReadOnlySpan<Entity> entities =
			[
				new Entity(0, 0),
				new Entity(1, 0),
				new Entity(2, 0),
				new Entity(3, 0),
				new Entity(4, 0)
			];
			ReadOnlySpan<Position3D> positions =
			[
				new Position3D(new Vector3(0.0f)),
				new Position3D(new Vector3(15.0f)),
				new Position3D(new Vector3(30.0f)),
				new Position3D(new Vector3(45.0f)),
				new Position3D(new Vector3(60.0f))
			];
			ReadOnlySpan<Rotation3D> rotations =
			[
				new Rotation3D(Quaternion.Zero),
				new Rotation3D(Quaternion.CreateFromYawPitchRoll(30.0f, 0.0f, 0.0f)),
				new Rotation3D(Quaternion.CreateFromYawPitchRoll(60.0f, 0.0f, 0.0f)),
				new Rotation3D(Quaternion.CreateFromYawPitchRoll(90.0f, 0.0f, 0.0f)),
				new Rotation3D(Quaternion.CreateFromYawPitchRoll(120.0f, 0.0f, 0.0f))
			];
			ReadOnlySpan<Scale3D> scales =
			[
				new Scale3D(new Vector3(1.0f)),
				new Scale3D(new Vector3(2.0f)),
				new Scale3D(new Vector3(3.0f)),
				new Scale3D(new Vector3(4.0f)),
				new Scale3D(new Vector3(5.0f))
			];
			ReadOnlySpan<Matrix4x4> matrices =
			[
				Matrix4x4.CreateTranslation(positions[0].Value) *
				Matrix4x4.CreateFromQuaternion(rotations[0].Value) *
				Matrix4x4.CreateScale(scales[0].Value),
				Matrix4x4.CreateTranslation(positions[1].Value) *
				Matrix4x4.CreateFromQuaternion(rotations[1].Value) *
				Matrix4x4.CreateScale(scales[1].Value),
				Matrix4x4.CreateTranslation(positions[2].Value) *
				Matrix4x4.CreateFromQuaternion(rotations[2].Value) *
				Matrix4x4.CreateScale(scales[2].Value),
				Matrix4x4.CreateTranslation(positions[3].Value) *
				Matrix4x4.CreateFromQuaternion(rotations[3].Value) *
				Matrix4x4.CreateScale(scales[3].Value),
				Matrix4x4.CreateTranslation(positions[4].Value) *
				Matrix4x4.CreateFromQuaternion(rotations[4].Value) *
				Matrix4x4.CreateScale(scales[4].Value),
			];

			for (int i = 0; i < 5; i++)
			{
				chunkWithMatrices.Add(entities[i]);
			}

			positions.CopyTo(chunkWithMatrices.GetComponents<Position3D>());
			rotations.CopyTo(chunkWithMatrices.GetComponents<Rotation3D>());
			scales.CopyTo(chunkWithMatrices.GetComponents<Scale3D>());
			matrices.CopyTo(chunkWithMatrices.GetComponents<Matrix4x4>());

			chunkWithoutMatrices.AddRange(chunkWithMatrices);

			Debug.Assert(chunkWithoutMatrices.Count == 5);
			Debug.Assert(chunkWithoutMatrices.GetEntities().SequenceEqual(entities));
			Debug.Assert(chunkWithoutMatrices.GetComponents<Position3D>().SequenceEqual(positions));
			Debug.Assert(chunkWithoutMatrices.GetComponents<Rotation3D>().SequenceEqual(rotations));
			Debug.Assert(chunkWithoutMatrices.GetComponents<Scale3D>().SequenceEqual(scales));

			chunkWithoutComponents.AddRange(chunkWithoutMatrices);

			Debug.Assert(chunkWithoutComponents.Count == 5);
			Debug.Assert(chunkWithoutComponents.GetEntities().SequenceEqual(entities));

			chunkWithMatrices.SetRange(0, chunkWithoutComponents);

			Debug.Assert(chunkWithMatrices.GetEntities().SequenceEqual(entities));
			Debug.Assert(chunkWithMatrices.GetComponents<Position3D>().Count(new Position3D()) == 5);
			Debug.Assert(chunkWithMatrices.GetComponents<Rotation3D>().Count(new Rotation3D()) == 5);
			Debug.Assert(chunkWithMatrices.GetComponents<Scale3D>().Count(new Scale3D()) == 5);
			Debug.Assert(chunkWithMatrices.GetComponents<Matrix4x4>().Count(new Matrix4x4()) == 5);

			chunkWithMatrices.SetRange(0, chunkWithoutMatrices);

			Debug.Assert(chunkWithMatrices.GetEntities().SequenceEqual(entities));
			Debug.Assert(chunkWithMatrices.GetComponents<Position3D>().SequenceEqual(positions));
			Debug.Assert(chunkWithMatrices.GetComponents<Rotation3D>().SequenceEqual(rotations));
			Debug.Assert(chunkWithMatrices.GetComponents<Scale3D>().SequenceEqual(scales));
			Debug.Assert(chunkWithMatrices.GetComponents<Matrix4x4>().Count(new Matrix4x4()) == 5);

			chunkWithoutMatrices.InsertRange(0, chunkWithMatrices);

			Debug.Assert(chunkWithoutMatrices.Count == 10);
			Debug.Assert(chunkWithoutMatrices.GetEntities().Count(entities) == 2);
			Debug.Assert(chunkWithoutMatrices.GetComponents<Position3D>().Count(positions) == 2);
			Debug.Assert(chunkWithoutMatrices.GetComponents<Rotation3D>().Count(rotations) == 2);
			Debug.Assert(chunkWithoutMatrices.GetComponents<Scale3D>().Count(scales) == 2);

			for (int i = 0; chunkWithoutComponents.Count < EntityArchetype.Base.ChunkCapacity; i++)
			{
				chunkWithoutComponents.Add(new Entity(i + 5, 0));
			}

			try
			{
				chunkWithoutMatrices.InsertRange(0, null!, 0, 0);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(0, chunkWithoutComponents, 0, chunkWithoutComponents.Count);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(int.MinValue, chunkWithoutComponents, 0, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(int.MaxValue, chunkWithoutComponents, 0, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(0, chunkWithoutComponents, int.MinValue, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(0, chunkWithoutComponents, int.MaxValue, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(0, chunkWithoutComponents, 0, int.MinValue);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.InsertRange(0, chunkWithoutComponents, 0, int.MaxValue);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, null!, 0, 0);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, chunkWithoutComponents, 0, chunkWithoutComponents.Count);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, chunkWithoutComponents, 0, chunkWithoutMatrices.Count + 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(int.MinValue, chunkWithoutComponents, 0, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(int.MaxValue, chunkWithoutComponents, 0, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, chunkWithoutComponents, int.MinValue, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, chunkWithoutComponents, int.MaxValue, 1);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, chunkWithoutComponents, 0, int.MinValue);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithoutMatrices.SetRange(0, chunkWithoutComponents, 0, int.MaxValue);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
	}
}
