using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeLookupMutationTest : ITestCase
	{
		public void Execute()
		{
			Action<State> callBack = CallBack;
			State state = new State();
			int[] typeCounters = [9, 8, 7, 6, 5, 4, 3, 2, 1];

			for (int i = 0; i < 50; i++)
			{
				ThreadPool.QueueUserWorkItem(callBack, state, false);
			}

			lock (state)
			{
				while (state.Counter > 0)
				{
					Monitor.Wait(state);
				}

				Debug.Assert(state.Lookup.Count == 10);
			}

			for (int i = 0; i < state.Lookup.Count; i++)
			{
				foreach (ComponentType type in state.Lookup[i].Key.ComponentTypes)
				{
					typeCounters[type.Id]--;
				}
			}

			foreach (int counter in typeCounters)
			{
				Debug.Assert(counter == 0);
			}
		}

		private static void CallBack(State state)
		{
			EntityArchetypeGrouping subGrouping = state.Lookup.GetGrouping(Array.Empty<ComponentType>());

			Debug.Assert(subGrouping == state.Lookup.GetGrouping(Array.Empty<ComponentType>()));
			Debug.Assert(subGrouping == state.Lookup.GetGrouping(Enumerable.Empty<ComponentType>()));
			Debug.Assert(subGrouping == state.Lookup.GetGrouping(ReadOnlySpan<ComponentType>.Empty));
			Debug.Assert(subGrouping == state.Lookup.GetGrouping(subGrouping.Key));
			Debug.Assert(subGrouping == state.Lookup.GetSubgrouping(subGrouping.Key, ComponentType.TypeOf<object>()));

			foreach (ComponentType type in state.Types)
			{
				EntityArchetypeGrouping superGrouping = state.Lookup.GetSupergrouping(subGrouping.Key, type);

				Debug.Assert(superGrouping.Key.Contains(type));
				Debug.Assert(superGrouping == state.Lookup.GetGrouping(superGrouping.Key.ComponentTypes.ToArray()));
				Debug.Assert(superGrouping == state.Lookup.GetGrouping((IEnumerable<ComponentType>)superGrouping.Key.ComponentTypes.ToArray()));
				Debug.Assert(superGrouping == state.Lookup.GetGrouping(superGrouping.Key.ComponentTypes));
				Debug.Assert(superGrouping == state.Lookup.GetGrouping(superGrouping.Key));
				Debug.Assert(superGrouping == state.Lookup.GetSupergrouping(subGrouping.Key, type));
				Debug.Assert(superGrouping == state.Lookup.GetSubgrouping(superGrouping.Key, ComponentType.TypeOf<object>()));
				Debug.Assert(subGrouping == state.Lookup.GetSubgrouping(superGrouping.Key, type));

				subGrouping = superGrouping;
			}

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
			public readonly EntityArchetypeLookup Lookup;
			public readonly ComponentType[] Types;
			public int Counter;

			public State()
			{
				Lookup = new EntityArchetypeLookup();
				Types =
				[
					ComponentType.TypeOf<Tag>(),
					ComponentType.TypeOf<Position2D>(),
					ComponentType.TypeOf<Rotation2D>(),
					ComponentType.TypeOf<Scale2D>(),
					ComponentType.TypeOf<Matrix3x2>(),
					ComponentType.TypeOf<Position3D>(),
					ComponentType.TypeOf<Rotation3D>(),
					ComponentType.TypeOf<Scale3D>(),
					ComponentType.TypeOf<Matrix4x4>()
				];
				Counter = 50;
			}
		}
	}
}
