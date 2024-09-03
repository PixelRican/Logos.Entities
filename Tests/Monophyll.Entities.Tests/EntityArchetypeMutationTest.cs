using System;
using System.Diagnostics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeMutationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype archetype = EntityArchetype.Base.Add(TypeOf<Position2D>());

			Debug.Assert(archetype.Contains(TypeOf<Position2D>()));

			archetype = archetype.Add(TypeOf<Rotation2D>());

			Debug.Assert(archetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Rotation2D>()));

			archetype = archetype.Remove(TypeOf<Position2D>());

			Debug.Assert(!archetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(archetype == archetype.Add(TypeOf<Rotation2D>()));
			Debug.Assert(archetype == archetype.Remove(TypeOf<Position2D>()));

			try
			{
				archetype.Add(null!);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				archetype.Remove(null!);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}
		}
	}
}
