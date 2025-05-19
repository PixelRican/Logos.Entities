using System;
using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityTableRangedMutationTest : ITestCase
	{
		public void Execute()
		{
			EntityTable tableA = new EntityTable(EntityArchetype.Create([
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>()]));
			EntityTable tableB = new EntityTable(tableA.Archetype.Add(ComponentType.TypeOf<Tag>()));
			EntityTable tableC = new EntityTable(tableA.Archetype.Add(ComponentType.TypeOf<Matrix3x2>()));

			for (int i = 0; i < 5; i++)
			{
				tableA.Add(new Entity(i, 0));
				tableB.Add(new Entity(i, 0));
				tableC.Add(new Entity(i, 0));
				tableA.GetComponents<Position2D>()[i] = new Position2D(new Vector2(i * 10.0f));
				tableA.GetComponents<Rotation2D>()[i] = new Rotation2D(i * MathF.PI / 4.0f);
				tableA.GetComponents<Scale2D>()[i] = new Scale2D(new Vector2(i + 1.0f));
				tableC.GetComponents<Matrix3x2>()[i] = Matrix3x2.Identity;
			}

			tableB.SetRange(0, tableA, 0, 5);

			Debug.Assert(tableB.GetEntities().SequenceEqual(tableA.GetEntities()));
			Debug.Assert(tableB.GetComponents<Position2D>().SequenceEqual(tableA.GetComponents<Position2D>()));
			Debug.Assert(tableB.GetComponents<Rotation2D>().SequenceEqual(tableA.GetComponents<Rotation2D>()));
			Debug.Assert(tableB.GetComponents<Scale2D>().SequenceEqual(tableA.GetComponents<Scale2D>()));

			tableC.SetRange(0, tableA, 0, 5);

			Debug.Assert(tableC.GetEntities().SequenceEqual(tableA.GetEntities()));
			Debug.Assert(tableC.GetComponents<Position2D>().SequenceEqual(tableA.GetComponents<Position2D>()));
			Debug.Assert(tableC.GetComponents<Rotation2D>().SequenceEqual(tableA.GetComponents<Rotation2D>()));
			Debug.Assert(tableC.GetComponents<Scale2D>().SequenceEqual(tableA.GetComponents<Scale2D>()));
			Debug.Assert(tableC.GetComponents<Matrix3x2>().Count(new Matrix3x2()) == 5);

			for (int i = 0; i < 5; i++)
			{
				tableA.Set(i, new Entity(i, -1));
			}

			tableA.SetRange(0, tableC, 0, 5);

			Debug.Assert(tableA.GetEntities().SequenceEqual(tableC.GetEntities()));
			Debug.Assert(tableA.GetComponents<Position2D>().SequenceEqual(tableC.GetComponents<Position2D>()));
			Debug.Assert(tableA.GetComponents<Rotation2D>().SequenceEqual(tableC.GetComponents<Rotation2D>()));
			Debug.Assert(tableA.GetComponents<Scale2D>().SequenceEqual(tableC.GetComponents<Scale2D>()));
		}
	}
}
