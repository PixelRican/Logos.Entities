using System;
using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal static class Program
	{
		public static void Main()
		{
			Stopwatch stopwatch = new Stopwatch();
			ITestCase[] tests =
			[
				new ComponentTypeCreationTest(),
				new ComponentTypeComparisonTest(),
				new EntityArchetypeCreationTest(),
				new EntityArchetypeCloningTest(),
				new EntityArchetypeChunkCreationTest(),
				new EntityArchetypeChunkMutationTest(),
				new EntityArchetypeChunkRangedMutationTest(),
				new EntityFilterMatchingTest(),
				new EntityArchetypeGroupingMutationTest(),
				new EntityArchetypeLookupMutationTest(),
				new EntityQueryEnumerationTest(),
				new EntityRegistryCreateDestroyEntityTest(),
				new EntityRegistryAddRemoveComponentTest()
			];

			Console.WriteLine($"Running {tests.Length} test cases...\n");

			foreach (ITestCase test in tests)
			{
				stopwatch.Restart();
				test.Execute();
				stopwatch.Stop();
				Console.WriteLine($"{test.GetType().Name} completed in {stopwatch.ElapsedMilliseconds} ms.");
			}

			Console.WriteLine("\nNo errors detected.");
		}
	}
}
