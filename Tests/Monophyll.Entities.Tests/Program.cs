using System;
using System.Diagnostics;

namespace Monophyll.Entities.Tests
{
	internal static class Program
	{
		public static void Main()
		{
			Stopwatch stopwatch = new();
			ReadOnlySpan<IUnitTest> unitTests =
			[
				new ComponentTypeFactoryTest(),
				new EntityArchetypeConstructorTest(),
				new EntityArchetypeAddRemoveTest(),
				new EntityArchetypeChunkPushPopTest(),
				new EntityArchetypeChunkPushPopRangeTest(),
				new EntityFilterConstructorTest(),
				new EntityFilterMatchTest(),
				new ComponentBitEqualityComparerTest(),
				new EntityRegistryCreateEntityArchetypeTest(),
				new EntityRegistryCreateDestroyEntityTest(),
				new EntityRegistryAddRemoveSetComponentTest(),
			];

			Console.WriteLine($"Running {unitTests.Length} test cases...\n");

			foreach (IUnitTest testCase in unitTests)
			{
				stopwatch.Restart();
				testCase.Run();
				stopwatch.Stop();
				Console.WriteLine($"{testCase.GetType().Name} completed in {stopwatch.ElapsedMilliseconds} ms.");
			}

			Console.WriteLine("\nNo errors detected.");
		}
	}
}
