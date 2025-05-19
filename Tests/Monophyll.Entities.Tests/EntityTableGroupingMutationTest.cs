using System;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityTableGroupingMutationTest : ITestCase
	{
		private static void Producer(State state)
		{
			state.Grouping.Add(state.Table);

			lock (state)
			{
				if (--state.Counter == 0)
				{
					Monitor.Pulse(state);
				}
			}
		}

		private static void Consumer(State state)
		{
			state.Grouping.Remove(state.Table);

			lock (state)
			{
				if (--state.Counter == 0)
				{
					Monitor.Pulse(state);
				}
			}
		}

		public void Execute()
		{
			Action<State> producer = Producer;
			Action<State> consumer = Consumer;
			State state = new State();

			for (int i = 0; i < 50; i++)
			{
				ThreadPool.QueueUserWorkItem(producer, state, false);
				ThreadPool.QueueUserWorkItem(consumer, state, false);
			}

			lock (state)
			{
				while (state.Counter > 0)
				{
					Monitor.Wait(state);
				}
			}
		}

		private sealed class State
		{
			public readonly EntityTable Table;
			public readonly EntityTableGrouping Grouping;
			public int Counter;

			public State()
			{
				Table = new EntityTable(EntityArchetype.Base);
				Grouping = new EntityTableGrouping(EntityArchetype.Base);

                for (int i = 0; i < 50; i++)
                {
                    Grouping.Add(Table);
                }

                Counter = 100;
			}
		}
	}
}
