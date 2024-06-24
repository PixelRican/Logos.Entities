namespace Monophyll.Entities.Test
{
	internal static class Program
	{
		public static void Main()
		{
			IUnitTest[] unitTests =
			[
				new ComponentTypeFactoryTest(),
				new EntityArchetypeConstructorTest(),
				new ComponentBitEqualityComparerTest()
			];

			foreach (IUnitTest testCase in unitTests)
			{
				testCase.Run();
			}
		}
	}
}
