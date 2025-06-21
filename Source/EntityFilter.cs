// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents a predicate that determines whether entity archetypes meet a set of criteria
    /// based on the component types they contain.
    /// </summary>
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

        /// <summary>
        /// Gets an entity filter instance that matches all entity archetypes.
        /// </summary>
        public static EntityFilter Universal
        {
            get => s_universal;
        }

        /// <summary>
        /// Gets a read-only span of component types that entity archetypes must contain in order
        /// to match with the entity filter.
        /// </summary>
        public ReadOnlySpan<ComponentType> RequiredComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_requiredComponentTypes);
        }

        /// <summary>
        /// Gets a read-only span of component types that entity archetypes must contain at least
        /// one instance of in order to match with the entity filter.
        /// </summary>
        /// 
        /// <remarks>
        /// If the read-only span is empty, this criteria will not be considered during matching.
        /// </remarks>
        public ReadOnlySpan<ComponentType> IncludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_includedComponentTypes);
        }

        /// <summary>
        /// Gets a read-only span of component types that entity archetypes must not contain in
        /// order to match with the entity filter.
        /// </summary>
        public ReadOnlySpan<ComponentType> ExcludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_excludedComponentTypes);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores information about the required component
        /// types defined by the entity filter.
        /// </summary>
        public ReadOnlySpan<uint> RequiredComponentBitmask
        {
            get => new ReadOnlySpan<uint>(m_requiredComponentBitmask);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores information about the included component
        /// types defined by the entity filter.
        /// </summary>
        public ReadOnlySpan<uint> IncludedComponentBitmask
        {
            get => new ReadOnlySpan<uint>(m_includedComponentBitmask);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores information about the excluded component
        /// types defined by the entity filter.
        /// </summary>
        public ReadOnlySpan<uint> ExcludedComponentBitmask
        {
            get => new ReadOnlySpan<uint>(m_excludedComponentBitmask);
        }

        /// <summary>
        /// Creates an entity filter that requires component types from the specified array.
        /// </summary>
        /// 
        /// <param name="requiredComponentTypes">
        /// The array of required component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter that requires component types from the array, or
        /// <see cref="Universal"/> if the array contains no component types.
        /// </returns>
        public static EntityFilter Create(ComponentType[] requiredComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask);
            }

            return s_universal;
        }

        /// <summary>
        /// Creates an entity filter that requires, includes, and excludes component types from the
        /// specified arrays.
        /// </summary>
        /// 
        /// <param name="requiredComponentTypes">
        /// The array of required component types.
        /// </param>
        /// 
        /// <param name="includedComponentTypes">
        /// The array of included component types.
        /// </param>
        /// 
        /// <param name="excludedComponentTypes">
        /// The array of excluded component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter that requires, includes, and excludes component types from the arrays,
        /// or <see cref="Universal"/> if the arrays contain no component types.
        /// </returns>
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

        /// <summary>
        /// Creates an entity filter that requires component types from the specified sequence.
        /// </summary>
        /// 
        /// <param name="requiredComponentTypes">
        /// The sequence of required component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter that requires component types from the sequence, or
        /// <see cref="Universal"/> if the sequence contains no component types.
        /// </returns>
        public static EntityFilter Create(IEnumerable<ComponentType> requiredComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask);
            }

            return s_universal;
        }

        /// <summary>
        /// Creates an entity filter that requires, includes, and excludes component types from the
        /// specified sequences.
        /// </summary>
        /// 
        /// <param name="requiredComponentTypes">
        /// The sequence of required component types.
        /// </param>
        /// 
        /// <param name="includedComponentTypes">
        /// The sequence of included component types.
        /// </param>
        /// 
        /// <param name="excludedComponentTypes">
        /// The sequence of excluded component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter that requires, includes, and excludes component types from the
        /// sequences, or <see cref="Universal"/> if the sequences contain no component types.
        /// </returns>
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

        /// <summary>
        /// Creates an entity filter that requires component types from the specified span.
        /// </summary>
        /// 
        /// <param name="requiredComponentTypes">
        /// The span of required component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter that requires component types from the span, or
        /// <see cref="Universal"/> if the span contains no component types.
        /// </returns>
        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBitmask))
            {
                return new EntityFilter(requiredTypes, requiredBitmask);
            }

            return s_universal;
        }

        /// <summary>
        /// Creates an entity filter that requires, includes, and excludes component types from the
        /// specified spans.
        /// </summary>
        /// 
        /// <param name="requiredComponentTypes">
        /// The span of required component types.
        /// </param>
        /// 
        /// <param name="includedComponentTypes">
        /// The span of included component types.
        /// </param>
        /// 
        /// <param name="excludedComponentTypes">
        /// The span of excluded component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter that requires, includes, and excludes component types from the spans,
        /// or <see cref="Universal"/> if the spans contain no component types.
        /// </returns>
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

            componentBitmask = new uint[componentTypes[^1].Identifier + 32 >> 5];

            int freeIndex = 0;
            ComponentType? previous = null;

            foreach (ComponentType current in componentTypes)
            {
                if (!ComponentType.Equals(previous, current))
                {
                    componentTypes[freeIndex++] = previous = current;
                    componentBitmask[current.Identifier >> 5] |= 1u << current.Identifier;
                }
            }

            Array.Resize(ref componentTypes, freeIndex);
            return true;
        }

        /// <summary>
        /// Creates an entity filter builder that contains the required component types from the
        /// specified array, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of required component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the required component types from the array.
        /// </returns>
        public static Builder Require(ComponentType[] componentTypes)
        {
            return new Builder().Require(componentTypes);
        }
        
        /// <summary>
        /// Creates an entity filter builder that contains the required component types from the
        /// specified sequence, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of required component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the required component types from the sequence.
        /// </returns>
        public static Builder Require(IEnumerable<ComponentType> componentTypes)
        {
            return new Builder().Require(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the required component types from the
        /// specified span, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of required component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the required component types from the span.
        /// </returns>
        public static Builder Require(ReadOnlySpan<ComponentType> componentTypes)
        {
            return new Builder().Require(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the included component types from the
        /// specified array, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of included component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the included component types from the array.
        /// </returns>
        public static Builder Include(ComponentType[] componentTypes)
        {
            return new Builder().Include(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the included component types from the
        /// specified sequence, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of included component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the included component types from the sequence.
        /// </returns>
        public static Builder Include(IEnumerable<ComponentType> componentTypes)
        {
            return new Builder().Include(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the included component types from the
        /// specified span, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of included component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the included component types from the span.
        /// </returns>
        public static Builder Include(ReadOnlySpan<ComponentType> componentTypes)
        {
            return new Builder().Include(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the excluded component types from the
        /// specified array, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of excluded component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the excluded component types from the array.
        /// </returns>
        public static Builder Exclude(ComponentType[] componentTypes)
        {
            return new Builder().Exclude(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the excluded component types from the
        /// specified sequence, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of excluded component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the excluded component types from the sequence.
        /// </returns>
        public static Builder Exclude(IEnumerable<ComponentType> componentTypes)
        {
            return new Builder().Exclude(componentTypes);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the excluded component types from the
        /// specified span, if any.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of excluded component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity filter builder that contains the excluded component types from the span.
        /// </returns>
        public static Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
        {
            return new Builder().Exclude(componentTypes);
        }

        /// <summary>
        /// Determines whether two specified entity filter objects have the same value.
        /// </summary>
        /// 
        /// <param name="a">
        /// The first entity filter to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <param name="b">
        /// The second entity filter to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the value of <paramref name="a"/> is the same as the value of
        /// <paramref name="b"/>; otherwise, <see langword="false"/>. If both <paramref name="a"/>
        /// and <paramref name="b"/> are <see langword="null"/>, the method returns
        /// <see langword="true"/>.
        /// </returns>
        public static bool Equals(EntityFilter? a, EntityFilter? b)
        {
            return a == b
                || a != null
                && b != null
                && a.RequiredComponentBitmask.SequenceEqual(b.RequiredComponentBitmask)
                && a.IncludedComponentBitmask.SequenceEqual(b.IncludedComponentBitmask)
                && a.ExcludedComponentBitmask.SequenceEqual(b.ExcludedComponentBitmask);
        }

        /// <summary>
        /// Indicates whether the entity filter requires the specified component type.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity archetype requires the component type,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Requires(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(RequiredComponentBitmask, componentType.Identifier);
        }

        /// <summary>
        /// Indicates whether the entity filter includes the specified component type.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity archetype includes the component type,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Includes(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(IncludedComponentBitmask, componentType.Identifier);
        }

        /// <summary>
        /// Indicates whether the entity filter excludes the specified component type.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity archetype excludes the component type,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Excludes(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(ExcludedComponentBitmask, componentType.Identifier);
        }

        /// <summary>
        /// Determines whether the specified entity archetype meets the criteria defined by the
        /// entity filter.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The entity archetype to compare against the criteria defined by the entity filter.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity archtype meets the criteria defined by the entity
        /// filter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Matches(EntityArchetype archetype)
        {
            ReadOnlySpan<uint> bitmask;
            return archetype != null
                && BitmaskOperations.Requires(RequiredComponentBitmask, bitmask = archetype.ComponentBitmask)
                && BitmaskOperations.Includes(IncludedComponentBitmask, bitmask)
                && BitmaskOperations.Excludes(ExcludedComponentBitmask, bitmask);
        }

        /// <summary>
        /// Creates an entity filter builder that contains the entity filter's required, included,
        /// and excluded component types.
        /// </summary>
        /// 
        /// <returns>
        /// An entity filter builder that contains the entity filter's required, included, and
        /// excluded component types.
        /// </returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
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

        /// <summary>
        /// Represents a writable buffer that can be converted into an entity filter instance
        /// without allocating extra memory.
        /// </summary>
        public sealed class Builder
        {
            private ComponentType[] m_requiredComponentTypes;
            private ComponentType[] m_includedComponentTypes;
            private ComponentType[] m_excludedComponentTypes;
            private uint[] m_requiredComponentBitmask;
            private uint[] m_includedComponentBitmask;
            private uint[] m_excludedComponentBitmask;

            /// <summary>
            /// Constructs an empty entity filter builder.
            /// </summary>
            public Builder()
            {
                Reset();
            }

            /// <summary>
            /// Constructs an entity filter builder that contains the specified entity filter's
            /// required, included, and excluded component types.
            /// </summary>
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

            /// <summary>
            /// Resets the entity filter builder's state.
            /// </summary>
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

            /// <summary>
            /// Modifies the entity filter builder to contain the required component types from the
            /// specified array, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The array of required component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Require(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the required component types from the
            /// specified sequence, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The sequence of required component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Require(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the required component types from the
            /// specified span, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The span of required component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Require(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the included component types from the
            /// specified array, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The array of included component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Include(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the included component types from the
            /// specified sequence, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The sequence of included component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Include(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the included component types from the
            /// specified span, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The span of included component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Include(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the excluded component types from the
            /// specified array, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The array of excluded component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Exclude(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the excluded component types from the
            /// specified sequence, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The sequence of excluded component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Exclude(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBitmask);
                return this;
            }

            /// <summary>
            /// Modifies the entity filter builder to contain the excluded component types from the
            /// specified span, if any.
            /// </summary>
            /// 
            /// <param name="componentTypes">
            /// The span of excluded component types.
            /// </param>
            /// 
            /// <returns>
            /// The calling entity builder instance.
            /// </returns>
            public Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBitmask);
                return this;
            }

            /// <summary>
            /// Creates an entity filter that requires, includes, and excludes component types
            /// specified by the entity filter builder.
            /// </summary>
            /// 
            /// <returns>
            /// An entity filter that requires, includes, and excludes component types specified by
            /// the entity filter builder.
            /// </returns>
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
