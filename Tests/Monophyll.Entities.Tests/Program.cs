using System;
using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal static class Program
	{
		public static void Main()
		{
			Stopwatch stopwatch = new();
			IUnitTest[] tests =
			[
				new ComponentTypeCreationTest(),
				new EntityArchetypeCreationTest(),
				new EntityArchetypeMutationTest(),
				new EntityArchetypeChunkCreationTest(),
				new EntityArchetypeChunkMutationTest(),
				new EntityArchetypeChunkRangedMutationTest(),
				new EntityArchetypeChunkGroupingMutationTest(),
				new EntityArchetypeChunkLookupMutationTest(),
				new EntityFilterCreationTest(),
				new EntityFilterMatchTest(),
				new EntityRegistryEntityManagementTest()
			];

			Console.WriteLine($"Running {tests.Length} test cases...\n");

			foreach (IUnitTest test in tests)
			{
				stopwatch.Restart();
				test.Run();
				stopwatch.Stop();
				Console.WriteLine($"{test.GetType().Name} completed in {stopwatch.ElapsedMilliseconds} ms.");
			}

			Console.WriteLine("\nNo errors detected.");
		}
	}
}
