using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityTableLookupMutationTest : ITestCase
	{
		public void Execute()
		{
			Action<State> callBack = CallBack;
			State state = new State();
			int[] counters = [9, 8, 7, 6, 5, 4, 3, 2, 1];

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
					counters[type.Id]--;
				}
			}

			foreach (int counter in counters)
			{
				Debug.Assert(counter == 0);
			}
		}

		private static void CallBack(State state)
		{
			EntityTableGrouping subgrouping = state.Lookup.GetGrouping(Array.Empty<ComponentType>());

			Debug.Assert(subgrouping == state.Lookup.GetGrouping(Array.Empty<ComponentType>()));
			Debug.Assert(subgrouping == state.Lookup.GetGrouping(Enumerable.Empty<ComponentType>()));
			Debug.Assert(subgrouping == state.Lookup.GetGrouping(ReadOnlySpan<ComponentType>.Empty));
			Debug.Assert(subgrouping == state.Lookup.GetGrouping(subgrouping.Key));
			Debug.Assert(subgrouping == state.Lookup.GetSubgrouping(subgrouping.Key, ComponentType.TypeOf<object>()));

			foreach (ComponentType type in state.Types)
			{
				EntityTableGrouping supergrouping = state.Lookup.GetSupergrouping(subgrouping.Key, type);

				Debug.Assert(supergrouping.Key.Contains(type));
				Debug.Assert(supergrouping == state.Lookup.GetGrouping(supergrouping.Key.ComponentTypes.ToArray()));
				Debug.Assert(supergrouping == state.Lookup.GetGrouping((IEnumerable<ComponentType>)supergrouping.Key.ComponentTypes.ToArray()));
				Debug.Assert(supergrouping == state.Lookup.GetGrouping(supergrouping.Key.ComponentTypes));
				Debug.Assert(supergrouping == state.Lookup.GetGrouping(supergrouping.Key));
				Debug.Assert(supergrouping == state.Lookup.GetSupergrouping(subgrouping.Key, type));
				Debug.Assert(supergrouping == state.Lookup.GetSubgrouping(supergrouping.Key, ComponentType.TypeOf<object>()));
				Debug.Assert(subgrouping == state.Lookup.GetSubgrouping(supergrouping.Key, type));

				subgrouping = supergrouping;
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
			public readonly EntityTableLookup Lookup;
			public readonly ComponentType[] Types;
			public int Counter;

			public State()
			{
				Lookup = new EntityTableLookup();
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
