using System;
using System.Diagnostics;
using System.Threading;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkGroupingMutationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetypeChunkGrouping grouping = new EntityArchetypeChunkGrouping(EntityArchetype.Create(0, 0));
			EntityArchetypeChunk chunk = new EntityArchetypeChunk(grouping.Key);
			int count = 1024;

            for (int i = Environment.ProcessorCount; i > 0; i--)
			{
				grouping.TryAdd(chunk);
			}

            for (int i = count >> 1; i > 0; i--)
			{
				ThreadPool.QueueUserWorkItem(Producer);
				ThreadPool.QueueUserWorkItem(Consumer);
			}

			lock (grouping)
			{
				while (Volatile.Read(ref count) > 0)
				{
					Monitor.Wait(grouping);
				}

				foreach (EntityArchetypeChunk item in grouping)
				{
					Debug.Assert(item == chunk);
					count++;
				}

				Debug.Assert(count == Environment.ProcessorCount);
			}

			void Producer(object? state)
			{
				Debug.Assert(grouping.TryAdd(chunk));

				if (Interlocked.Decrement(ref count) == 0)
				{
					lock (grouping)
					{
						Monitor.Pulse(grouping);
					}
				}
			}

			void Consumer(object? state)
			{
				Debug.Assert(grouping.TryTake(out _));

				if (Interlocked.Decrement(ref count) == 0)
				{
					lock (grouping)
					{
						Monitor.Pulse(grouping);
					}
				}
			}
		}
	}
}
