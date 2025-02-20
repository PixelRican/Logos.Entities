using System;
using System.Diagnostics;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeGroupingMutationTest : ITestCase
	{
		public void Execute()
		{
			Action<State> producer = Producer;
			Action<State> consumer = Consumer;
			State state = new State();

			Debug.Assert(state.Grouping.TryAdd(new EntityArchetypeChunk(state.Grouping.Key.Clone(0))));
			Debug.Assert(state.Grouping.TryTake(out EntityArchetypeChunk? chunk));
			Debug.Assert(state.Grouping.TryAdd(new EntityArchetypeChunk(state.Grouping.Key)));
			Debug.Assert(state.Grouping.TryTake(out chunk));

			for (int i = 0; i < 50; i++)
			{
				state.Grouping.TryAdd(chunk);
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

		private static void Producer(State state)
		{
			Debug.Assert(state.Grouping.TryPeek(out EntityArchetypeChunk? chunk));
			Debug.Assert(state.Grouping.TryAdd(chunk!));

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
			Debug.Assert(state.Grouping.TryTake(out _));

			lock (state)
			{
				if (--state.Counter == 0)
				{
					Monitor.Pulse(state);
				}
			}
		}

		private sealed class State
		{
			public readonly EntityArchetypeGrouping Grouping;
			public int Counter;

			public State()
			{
				Grouping = new EntityArchetypeGrouping(EntityArchetype.Create(0));
				Counter = 100;
			}
		}
	}
}
