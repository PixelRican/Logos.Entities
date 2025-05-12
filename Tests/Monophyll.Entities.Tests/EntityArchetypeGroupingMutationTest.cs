using System;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeGroupingMutationTest : ITestCase
	{
		private static void Producer(State state)
		{
			state.Grouping.Add(state.Chunk);

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
			state.Grouping.Remove(state.Chunk);

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
				state.Grouping.Add(state.Chunk);
			}

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
			public readonly EntityArchetypeChunk Chunk;
			public readonly EntityArchetypeGrouping Grouping;
			public int Counter;

			public State()
			{
				Chunk = new EntityArchetypeChunk(EntityArchetype.Base);
				Grouping = new EntityArchetypeGrouping(EntityArchetype.Base);
				Counter = 100;
			}
		}
	}
}
