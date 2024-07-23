using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public readonly struct EntityQueryResults : IReadOnlyList<Entity>, IEquatable<EntityQueryResults>
	{
		private readonly EntityArchetypeChunk m_chunk;

		public EntityQueryResults(EntityArchetypeChunk chunk)
		{
			m_chunk = chunk;
		}

		public static EntityQueryResults Null
		{
			get => default;
		}

		public EntityArchetype Archetype
		{
			get => m_chunk.Archetype;
		}

		public int Count
		{
			get => m_chunk.Count;
		}

		public int Version
		{
			get => m_chunk.Version;
		}

		public bool IsValid
		{
			get => m_chunk != null;
		}

		public Entity this[int index]
		{
			get => m_chunk[index];
		}

		public bool Equals(EntityQueryResults other)
		{
			return m_chunk == other.m_chunk;
		}

		public override bool Equals(object? obj)
		{
			return obj is EntityQueryResults other && Equals(other);
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(m_chunk);
		}

		public Span<byte> GetComponentData(ComponentType componentType)
		{
			return m_chunk.GetComponentData(componentType);
		}

		public bool TryGetComponentData(ComponentType componentType, out Span<byte> result)
		{
			return m_chunk.TryGetComponentData(componentType, out result);
		}

		public Span<T> GetComponents<T>() where T : unmanaged
		{
			return m_chunk.GetComponents<T>();
		}

		public bool TryGetComponents<T>(out Span<T> result) where T : unmanaged
		{
			return m_chunk.TryGetComponents(out result);
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return m_chunk.GetEntities();
		}

		public EntityArchetypeChunk.Enumerator GetEnumerator()
		{
			return m_chunk.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_chunk.GetEnumerator();
		}

		IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
		{
			return m_chunk.GetEnumerator();
		}

		public static bool operator ==(EntityQueryResults left, EntityQueryResults right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(EntityQueryResults left, EntityQueryResults right)
		{
			return !left.Equals(right);
		}
	}
}
