// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Monophyll.Entities
{
    public sealed class EntityFilter : IEquatable<EntityFilter>
    {
        private static readonly EntityFilter s_universal = new EntityFilter();

        private readonly ComponentType[] m_requiredComponentTypes;
        private readonly ComponentType[] m_includedComponentTypes;
        private readonly ComponentType[] m_excludedComponentTypes;
        private readonly uint[] m_requiredComponentBitmask;
        private readonly uint[] m_includedComponentBitmask;
        private readonly uint[] m_excludedComponentBitmask;

        private EntityFilter()
        {
            m_requiredComponentTypes = Array.Empty<ComponentType>();
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBitmask = Array.Empty<uint>();
            m_includedComponentBitmask = Array.Empty<uint>();
            m_excludedComponentBitmask = Array.Empty<uint>();
        }

        private EntityFilter(ComponentType[] requiredComponentTypes, uint[] requiredComponentBitmask)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBitmask = requiredComponentBitmask;
            m_includedComponentBitmask = Array.Empty<uint>();
            m_excludedComponentBitmask = Array.Empty<uint>();
        }

        private EntityFilter(ComponentType[] requiredComponentTypes, uint[] requiredComponentBitmask,
                             ComponentType[] includedComponentTypes, uint[] includedComponentBitmask,
                             ComponentType[] excludedComponentTypes, uint[] excludedComponentBitmask)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = includedComponentTypes;
            m_excludedComponentTypes = excludedComponentTypes;
            m_requiredComponentBitmask = requiredComponentBitmask;
            m_includedComponentBitmask = includedComponentBitmask;
            m_excludedComponentBitmask = excludedComponentBitmask;
        }

        public static EntityFilter Universal
        {
            get => s_universal;
        }

        public ReadOnlySpan<ComponentType> RequiredComponentTypes
        {
            get => m_requiredComponentTypes;
        }

        public ReadOnlySpan<ComponentType> IncludedComponentTypes
        {
            get => m_includedComponentTypes;
        }

        public ReadOnlySpan<ComponentType> ExcludedComponentTypes
        {
            get => m_excludedComponentTypes;
        }

        public ReadOnlySpan<uint> RequiredComponentBitmask
        {
            get => m_requiredComponentBitmask;
        }

        public ReadOnlySpan<uint> IncludedComponentBitmask
        {
            get => m_includedComponentBitmask;
        }

        public ReadOnlySpan<uint> ExcludedComponentBitmask
        {
            get => m_excludedComponentBitmask;
        }

        public static EntityFilter Create(ComponentType[] requiredComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask);
            }

            return s_universal;
        }

        public static EntityFilter Create(ComponentType[] requiredComponentTypes,
                                          ComponentType[] includedComponentTypes,
                                          ComponentType[] excludedComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);
            ArgumentNullException.ThrowIfNull(includedComponentTypes);
            ArgumentNullException.ThrowIfNull(excludedComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask) |
                TryBuild(includedComponentTypes, out ComponentType[] includedTypes, out uint[] includedBitmask) |
                TryBuild(excludedComponentTypes, out ComponentType[] excludedTypes, out uint[] excludedBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask,
                                        includedTypes, includedBitmask,
                                        excludedTypes, excludedBitmask);
            }

            return s_universal;
        }

        public static EntityFilter Create(IEnumerable<ComponentType> requiredComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask);
            }

            return s_universal;
        }

        public static EntityFilter Create(IEnumerable<ComponentType> requiredComponentTypes,
                                          IEnumerable<ComponentType> includedComponentTypes,
                                          IEnumerable<ComponentType> excludedComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);
            ArgumentNullException.ThrowIfNull(includedComponentTypes);
            ArgumentNullException.ThrowIfNull(excludedComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask) |
                TryBuild(includedComponentTypes, out ComponentType[] includedTypes, out uint[] includedBitmask) |
                TryBuild(excludedComponentTypes, out ComponentType[] excludedTypes, out uint[] excludedBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask,
                                        includedTypes, includedBitmask,
                                        excludedTypes, excludedBitmask);
            }

            return s_universal;
        }

        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask);
            }

            return s_universal;
        }

        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredComponentTypes,
                                          ReadOnlySpan<ComponentType> includedComponentTypes,
                                          ReadOnlySpan<ComponentType> excludedComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask) |
                TryBuild(includedComponentTypes, out ComponentType[] includedTypes, out uint[] includedBitmask) |
                TryBuild(excludedComponentTypes, out ComponentType[] excludedTypes, out uint[] excludedBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask,
                                        includedTypes, includedBitmask,
                                        excludedTypes, excludedBitmask);
            }

            return s_universal;
        }

        private static bool TryBuild(ComponentType[] arguments,
            out ComponentType[] componentTypes, out uint[] componentBitmask)
        {
            if (arguments.Length > 0)
            {
                componentTypes = new ComponentType[arguments.Length];
                Array.Copy(arguments, componentTypes, arguments.Length);
                return TryFinalizeBuild(ref componentTypes, out componentBitmask);
            }

            componentTypes = Array.Empty<ComponentType>();
            componentBitmask = Array.Empty<uint>();
            return false;
        }

        private static bool TryBuild(IEnumerable<ComponentType> arguments,
            out ComponentType[] componentTypes, out uint[] componentBitmask)
        {
            componentTypes = arguments.ToArray();

            if (componentTypes.Length > 0)
            {
                return TryFinalizeBuild(ref componentTypes, out componentBitmask);
            }

            componentBitmask = Array.Empty<uint>();
            return false;
        }

        private static bool TryBuild(ReadOnlySpan<ComponentType> arguments,
            out ComponentType[] componentTypes, out uint[] componentBitmask)
        {
            componentTypes = arguments.ToArray();

            if (componentTypes.Length > 0)
            {
                return TryFinalizeBuild(ref componentTypes, out componentBitmask);
            }

            componentBitmask = Array.Empty<uint>();
            return false;
        }

        private static bool TryFinalizeBuild(ref ComponentType[] componentTypes, out uint[] componentBitmask)
        {
            Array.Sort(componentTypes);

            if (componentTypes[^1] == null)
            {
                componentTypes = Array.Empty<ComponentType>();
                componentBitmask = Array.Empty<uint>();
                return false;
            }

            componentBitmask = new uint[componentTypes[^1].ID + 32 >> 5];

            int freeIndex = 0;
            ComponentType? previous = null;

            foreach (ComponentType current in componentTypes)
            {
                if (!ComponentType.Equals(previous, current))
                {
                    componentTypes[freeIndex++] = previous = current;
                    componentBitmask[current.ID >> 5] |= 1u << current.ID;
                }
            }

            Array.Resize(ref componentTypes, freeIndex);
            return true;
        }

        public static Builder Require(ComponentType[] componentTypes)
        {
            return new Builder().Require(componentTypes);
        }

        public static Builder Require(IEnumerable<ComponentType> componentTypes)
        {
            return new Builder().Require(componentTypes);
        }

        public static Builder Require(ReadOnlySpan<ComponentType> componentTypes)
        {
            return new Builder().Require(componentTypes);
        }

        public static Builder Include(ComponentType[] componentTypes)
        {
            return new Builder().Include(componentTypes);
        }

        public static Builder Include(IEnumerable<ComponentType> componentTypes)
        {
            return new Builder().Include(componentTypes);
        }

        public static Builder Include(ReadOnlySpan<ComponentType> componentTypes)
        {
            return new Builder().Include(componentTypes);
        }

        public static Builder Exclude(ComponentType[] componentTypes)
        {
            return new Builder().Exclude(componentTypes);
        }

        public static Builder Exclude(IEnumerable<ComponentType> componentTypes)
        {
            return new Builder().Exclude(componentTypes);
        }

        public static Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
        {
            return new Builder().Exclude(componentTypes);
        }

        public static bool Equals(EntityFilter? a, EntityFilter? b)
        {
            return a == b
                || a != null
                && b != null
                && a.RequiredComponentBitmask.SequenceEqual(b.RequiredComponentBitmask)
                && a.IncludedComponentBitmask.SequenceEqual(b.IncludedComponentBitmask)
                && a.ExcludedComponentBitmask.SequenceEqual(b.ExcludedComponentBitmask);
        }

        public bool Equals(EntityFilter? other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(this, obj as EntityFilter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BitmaskOperations.GetHashCode(RequiredComponentBitmask),
                                    BitmaskOperations.GetHashCode(IncludedComponentBitmask),
                                    BitmaskOperations.GetHashCode(ExcludedComponentBitmask));
        }

        public bool Requires(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Contains(RequiredComponentBitmask, componentType.ID);
        }

        public bool Includes(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Contains(IncludedComponentBitmask, componentType.ID);
        }

        public bool Excludes(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Contains(ExcludedComponentBitmask, componentType.ID);
        }

        public bool Matches(EntityArchetype archetype)
        {
            ReadOnlySpan<uint> bitmask;
            return archetype != null
                && BitmaskOperations.Requires(RequiredComponentBitmask, bitmask = archetype.ComponentBitmask)
                && BitmaskOperations.Includes(IncludedComponentBitmask, bitmask)
                && BitmaskOperations.Excludes(ExcludedComponentBitmask, bitmask);
        }

        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        public sealed class Builder
        {
            private ComponentType[] m_requiredComponentTypes;
            private ComponentType[] m_includedComponentTypes;
            private ComponentType[] m_excludedComponentTypes;
            private uint[] m_requiredComponentBitmask;
            private uint[] m_includedComponentBitmask;
            private uint[] m_excludedComponentBitmask;

            public Builder()
            {
                Reset();
            }

            public Builder(EntityFilter filter)
            {
                ArgumentNullException.ThrowIfNull(filter);

                m_requiredComponentTypes = filter.m_requiredComponentTypes;
                m_includedComponentTypes = filter.m_includedComponentTypes;
                m_excludedComponentTypes = filter.m_excludedComponentTypes;
                m_requiredComponentBitmask = filter.m_requiredComponentBitmask;
                m_includedComponentBitmask = filter.m_includedComponentBitmask;
                m_excludedComponentBitmask = filter.m_excludedComponentBitmask;
            }

            [MemberNotNull(nameof(m_requiredComponentTypes), nameof(m_requiredComponentBitmask),
                           nameof(m_includedComponentTypes), nameof(m_includedComponentBitmask),
                           nameof(m_excludedComponentTypes), nameof(m_excludedComponentBitmask))]
            public void Reset()
            {
                m_requiredComponentTypes = Array.Empty<ComponentType>();
                m_includedComponentTypes = Array.Empty<ComponentType>();
                m_excludedComponentTypes = Array.Empty<ComponentType>();
                m_requiredComponentBitmask = Array.Empty<uint>();
                m_includedComponentBitmask = Array.Empty<uint>();
                m_excludedComponentBitmask = Array.Empty<uint>();
            }

            public Builder Require(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBitmask);
                return this;
            }

            public Builder Require(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBitmask);
                return this;
            }

            public Builder Require(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBitmask);
                return this;
            }

            public Builder Include(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBitmask);
                return this;
            }

            public Builder Include(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBitmask);
                return this;
            }

            public Builder Include(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBitmask);
                return this;
            }

            public Builder Exclude(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBitmask);
                return this;
            }

            public Builder Exclude(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBitmask);
                return this;
            }

            public Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBitmask);
                return this;
            }

            public EntityFilter Build()
            {
                if (m_requiredComponentTypes.Length > 0 ||
                    m_includedComponentTypes.Length > 0 ||
                    m_excludedComponentTypes.Length > 0)
                {
                    return new EntityFilter(m_requiredComponentTypes, m_requiredComponentBitmask,
                                            m_includedComponentTypes, m_includedComponentBitmask,
                                            m_excludedComponentTypes, m_excludedComponentBitmask);
                }

                return s_universal;
            }
        }
    }
}
