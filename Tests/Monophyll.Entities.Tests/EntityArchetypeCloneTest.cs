using System;
using System.Diagnostics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeCloneTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype archetype = EntityArchetype.Create([TypeOf<Position2D>()], 0, 0);

			Debug.Assert(archetype.Contains(TypeOf<Position2D>()));

			archetype = archetype.CloneWith(TypeOf<Rotation2D>(), 0, 0);

			Debug.Assert(archetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Rotation2D>()));

			archetype = archetype.CloneWithout(TypeOf<Position2D>(), 0, 0);

			Debug.Assert(!archetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Rotation2D>()));

			try
			{
				archetype.CloneWith(null!, 0, 0);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				archetype.CloneWithout(null!, 0, 0);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}
		}
	}
}
