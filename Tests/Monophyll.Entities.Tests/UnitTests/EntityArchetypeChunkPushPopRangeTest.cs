using System;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkPushPopRangeTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetypeChunk chunkWithMatrix = new(new EntityArchetype([
				TypeOf<Position2D>(),
				TypeOf<Rotation2D>(),
				TypeOf<Scale2D>(),
				TypeOf<Matrix3x2>()
			]));
			EntityArchetypeChunk chunkWithoutMatrix = new(new EntityArchetype([
				TypeOf<Position2D>(),
				TypeOf<Rotation2D>(),
				TypeOf<Scale2D>()
			]));

			ReadOnlySpan<Entity> entitiesToCompare =
			[
				new Entity(0, 0),
				new Entity(1, 0),
				new Entity(2, 0),
				new Entity(3, 0),
				new Entity(4, 0)
			];

			ReadOnlySpan<Position2D> positionsToCompare =
			[
				new Position2D(new Vector2(100.0f, 20.0f)),
				new Position2D(new Vector2(120.0f, 125.0f)),
				new Position2D(new Vector2(-205.0f, 85.0f)),
				new Position2D(new Vector2(15.0f, -320.0f)),
				new Position2D(new Vector2(-275.0f, -25.0f))
			];

			ReadOnlySpan<Rotation2D> rotationsToCompare =
			[
				new Rotation2D(0.0f),
				new Rotation2D(45.0f),
				new Rotation2D(-90.0f),
				new Rotation2D(-135.0f),
				new Rotation2D(180.0f),
			];

			ReadOnlySpan<Scale2D> scalesToCompare =
			[
				new Scale2D(new Vector2(1.0f)),
				new Scale2D(new Vector2(1.5f)),
				new Scale2D(new Vector2(1.75f)),
				new Scale2D(new Vector2(2.25f)),
				new Scale2D(new Vector2(3.125f)),
			];

			ReadOnlySpan<Matrix3x2> matricesToCompare =
			[
				Matrix3x2.CreateTranslation(positionsToCompare[0].Value) *
				Matrix3x2.CreateRotation(rotationsToCompare[0].Value) *
				Matrix3x2.CreateScale(scalesToCompare[0].Value),
				Matrix3x2.CreateTranslation(positionsToCompare[1].Value) *
				Matrix3x2.CreateRotation(rotationsToCompare[1].Value) *
				Matrix3x2.CreateScale(scalesToCompare[1].Value),
				Matrix3x2.CreateTranslation(positionsToCompare[2].Value) *
				Matrix3x2.CreateRotation(rotationsToCompare[2].Value) *
				Matrix3x2.CreateScale(scalesToCompare[2].Value),
				Matrix3x2.CreateTranslation(positionsToCompare[3].Value) *
				Matrix3x2.CreateRotation(rotationsToCompare[3].Value) *
				Matrix3x2.CreateScale(scalesToCompare[3].Value),
				Matrix3x2.CreateTranslation(positionsToCompare[4].Value) *
				Matrix3x2.CreateRotation(rotationsToCompare[4].Value) *
				Matrix3x2.CreateScale(scalesToCompare[4].Value),
			];

			for (int i = 0; i < 5; i++)
			{
				chunkWithoutMatrix.Push(entitiesToCompare[i]);
			}

			positionsToCompare.CopyTo(chunkWithoutMatrix.GetComponents<Position2D>());
			rotationsToCompare.CopyTo(chunkWithoutMatrix.GetComponents<Rotation2D>());
			scalesToCompare.CopyTo(chunkWithoutMatrix.GetComponents<Scale2D>());

			try
			{
				chunkWithMatrix.PushRange(chunkWithoutMatrix, 0, -1);
				Debug.Fail("PushRange should not allow negative counts.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PushRange(chunkWithoutMatrix, -1, 5);
				Debug.Fail("PushRange should not allow negative chunk indices.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PushRange(chunkWithoutMatrix, -1, -5);
				Debug.Fail("PushRange should not allow negative indices or counts.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PushRange(chunkWithoutMatrix, 0, 6);
				Debug.Fail("PushRange should not allow counts that exceed the bounds of the other chunk.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PushRange(chunkWithoutMatrix, 1, 5);
				Debug.Fail("PushRange should not allow indices that exceed the bounds of the other chunk.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			chunkWithMatrix.PushRange(chunkWithoutMatrix, 0, 5);

			Debug.Assert(chunkWithMatrix.Count == 5);
			Debug.Assert(chunkWithMatrix.GetEntities().SequenceEqual(entitiesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Position2D>().SequenceEqual(positionsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Rotation2D>().SequenceEqual(rotationsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Scale2D>().SequenceEqual(scalesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Matrix3x2>().Count(new Matrix3x2()) == 5);

			matricesToCompare.CopyTo(chunkWithMatrix.GetComponents<Matrix3x2>());
			chunkWithMatrix.PushRange(chunkWithMatrix, 0, 5);

			Debug.Assert(chunkWithMatrix.Count == 10);
			Debug.Assert(chunkWithMatrix.GetEntities()[5..].SequenceEqual(entitiesToCompare));
			Debug.Assert(chunkWithMatrix.GetEntities()[..5].SequenceEqual(entitiesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Position2D>()[5..].SequenceEqual(positionsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Position2D>()[..5].SequenceEqual(positionsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Rotation2D>()[5..].SequenceEqual(rotationsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Rotation2D>()[..5].SequenceEqual(rotationsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Scale2D>()[5..].SequenceEqual(scalesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Scale2D>()[..5].SequenceEqual(scalesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Matrix3x2>()[5..].SequenceEqual(matricesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Matrix3x2>()[..5].SequenceEqual(matricesToCompare));

			for (int i = 0; i < 5; i++)
			{
				chunkWithoutMatrix.Push(new Entity());
			}

			try
			{
				chunkWithMatrix.PopRange(chunkWithoutMatrix, 0, -1);
				Debug.Fail("PopRange should not allow negative counts.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PopRange(chunkWithoutMatrix, -1, 5);
				Debug.Fail("PopRange should not allow negative chunk indices.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PopRange(chunkWithoutMatrix, -1, -5);
				Debug.Fail("PopRange should not allow negative indices or counts.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PopRange(chunkWithoutMatrix, 0, 11);
				Debug.Fail("PopRange should not allow counts that exceed the bounds of the other chunk.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			try
			{
				chunkWithMatrix.PopRange(chunkWithoutMatrix, 6, 5);
				Debug.Fail("PopRange should not allow indices that exceed the bounds of the other chunk.");
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			chunkWithMatrix.PopRange(chunkWithoutMatrix, 5, 5);

			Debug.Assert(chunkWithMatrix.Count == 5);
			Debug.Assert(chunkWithMatrix.GetEntities().SequenceEqual(entitiesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Position2D>().SequenceEqual(positionsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Rotation2D>().SequenceEqual(rotationsToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Scale2D>().SequenceEqual(scalesToCompare));
			Debug.Assert(chunkWithMatrix.GetComponents<Matrix3x2>().SequenceEqual(matricesToCompare));

			Debug.Assert(chunkWithoutMatrix.GetEntities()[5..].SequenceEqual(entitiesToCompare));
			Debug.Assert(chunkWithoutMatrix.GetEntities()[..5].SequenceEqual(entitiesToCompare));
			Debug.Assert(chunkWithoutMatrix.GetComponents<Position2D>()[5..].SequenceEqual(positionsToCompare));
			Debug.Assert(chunkWithoutMatrix.GetComponents<Position2D>()[..5].SequenceEqual(positionsToCompare));
			Debug.Assert(chunkWithoutMatrix.GetComponents<Rotation2D>()[5..].SequenceEqual(rotationsToCompare));
			Debug.Assert(chunkWithoutMatrix.GetComponents<Rotation2D>()[..5].SequenceEqual(rotationsToCompare));
			Debug.Assert(chunkWithoutMatrix.GetComponents<Scale2D>()[5..].SequenceEqual(scalesToCompare));
			Debug.Assert(chunkWithoutMatrix.GetComponents<Scale2D>()[..5].SequenceEqual(scalesToCompare));
		}
	}
}
