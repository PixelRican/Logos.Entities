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
				new EntityArchetypeAddRemoveTest(),
				new EntityTableCreationTest(),
				new EntityTableMutationTest(),
				new EntityTableRangedMutationTest(),
				new EntityFilterMatchingTest(),
				new EntityTableGroupingMutationTest(),
				new EntityTableLookupMutationTest(),
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
