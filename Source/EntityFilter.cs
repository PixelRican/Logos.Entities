// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a predicate that determines whether archetypes match a set of criteria in terms
    /// of required, included, and excluded component types.
    /// </summary>
    public sealed class EntityFilter : IEquatable<EntityFilter>
    {
        private static readonly EntityFilter s_universal = new EntityFilter();

        private readonly ComponentType[] m_requiredComponentTypes;
        private readonly ComponentType[] m_includedComponentTypes;
        private readonly ComponentType[] m_excludedComponentTypes;
        private readonly int[] m_requiredComponentBitmask;
        private readonly int[] m_includedComponentBitmask;
        private readonly int[] m_excludedComponentBitmask;

        private EntityFilter()
        {
            m_requiredComponentTypes = Array.Empty<ComponentType>();
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBitmask = Array.Empty<int>();
            m_includedComponentBitmask = Array.Empty<int>();
            m_excludedComponentBitmask = Array.Empty<int>();
        }

        private EntityFilter(ComponentType[] requiredComponentTypes, int[] requiredComponentBitmask)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBitmask = requiredComponentBitmask;
            m_includedComponentBitmask = Array.Empty<int>();
            m_excludedComponentBitmask = Array.Empty<int>();
        }

        private EntityFilter(ComponentType[] requiredComponentTypes, int[] requiredComponentBitmask,
                             ComponentType[] includedComponentTypes, int[] includedComponentBitmask,
                             ComponentType[] excludedComponentTypes, int[] excludedComponentBitmask)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = includedComponentTypes;
            m_excludedComponentTypes = excludedComponentTypes;
            m_requiredComponentBitmask = requiredComponentBitmask;
            m_includedComponentBitmask = includedComponentBitmask;
            m_excludedComponentBitmask = excludedComponentBitmask;
        }

        /// <summary>
        /// Gets an <see cref="EntityFilter"/> that matches all archetypes.
        /// </summary>
        /// <returns>
        /// An <see cref="EntityFilter"/> that matches all archetypes.
        /// </returns>
        public static EntityFilter Universal
        {
            get => s_universal;
        }

        /// <summary>
        /// Gets a read-only span of component types that archetypes must contain in order to match
        /// with the <see cref="EntityFilter"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that archetypes must contain in order to match with
        /// the <see cref="EntityFilter"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> RequiredComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_requiredComponentTypes);
        }

        /// <summary>
        /// Gets a read-only span of component types that archetypes must contain at least one
        /// instance of in order to match with the <see cref="EntityFilter"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that, if not empty, archetypes must contain at least
        /// one instance of in order to match with the <see cref="EntityFilter"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> IncludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_includedComponentTypes);
        }

        /// <summary>
        /// Gets a read-only span of component types that archetypes must not contain in order to
        /// match with the <see cref="EntityFilter"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that archetypes must not contain in order to match
        /// with the <see cref="EntityFilter"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> ExcludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_excludedComponentTypes);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores flags indicating whether the
        /// <see cref="EntityFilter"/> requires a specific component type.
        /// </summary>
        /// <returns>
        /// A read-only bitmask that compactly stores flags indicating whether the
        /// <see cref="EntityFilter"/> requires a specific component type.
        /// </returns>
        public ReadOnlySpan<int> RequiredComponentBitmask
        {
            get => new ReadOnlySpan<int>(m_requiredComponentBitmask);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores flags indicating whether the
        /// <see cref="EntityFilter"/> includes a specific component type.
        /// </summary>
        /// <returns>
        /// A read-only bitmask that compactly stores flags indicating whether the
        /// <see cref="EntityFilter"/> includes a specific component type.
        /// </returns>
        public ReadOnlySpan<int> IncludedComponentBitmask
        {
            get => new ReadOnlySpan<int>(m_includedComponentBitmask);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores flags indicating whether the
        /// <see cref="EntityFilter"/> excludes a specific component type.
        /// </summary>
        /// <returns>
        /// A read-only bitmask that compactly stores flags indicating whether the
        /// <see cref="EntityFilter"/> excludes a specific component type.
        /// </returns>
        public ReadOnlySpan<int> ExcludedComponentBitmask
        {
            get => new ReadOnlySpan<int>(m_excludedComponentBitmask);
        }

        /// <summary>
        /// Creates an <see cref="EntityFilter"/> that requires specific component types from the
        /// specified array.
        /// </summary>
        /// <param name="requiredArray">
        /// The array of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityFilter"/> that requires specific component types from
        /// <paramref name="requiredArray"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredArray"/> does not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="requiredArray"/> is <see langword="null"/>.
        /// </exception>
        public static EntityFilter Create(ComponentType[] requiredArray)
        {
            ArgumentNullException.ThrowIfNull(requiredArray);
            return CreateInstance(new ReadOnlySpan<ComponentType>(requiredArray).ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityFilter"/> that requires, includes, and excludes specific
        /// component types from the specified arrays.
        /// </summary>
        /// <param name="requiredArray">
        /// The array of required component types.
        /// </param>
        /// <param name="includedArray">
        /// The array of included component types.
        /// </param>
        /// <param name="excludedArray">
        /// The array of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityFilter"/> that requires, includes, and excludes specific component
        /// types from <paramref name="requiredArray"/>, <paramref name="includedArray"/>, and
        /// <paramref name="excludedArray"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredArray"/>, <paramref name="includedArray"/>, and
        /// <paramref name="excludedArray"/> do not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="requiredArray"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="includedArray"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="excludedArray"/> is <see langword="null"/>.
        /// </exception>
        public static EntityFilter Create(ComponentType[] requiredArray,
                                          ComponentType[] includedArray,
                                          ComponentType[] excludedArray)
        {
            ArgumentNullException.ThrowIfNull(requiredArray);
            ArgumentNullException.ThrowIfNull(includedArray);
            ArgumentNullException.ThrowIfNull(excludedArray);

            return CreateInstance(new ReadOnlySpan<ComponentType>(requiredArray).ToArray(),
                                  new ReadOnlySpan<ComponentType>(includedArray).ToArray(),
                                  new ReadOnlySpan<ComponentType>(excludedArray).ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityFilter"/> that requires specific component types from the
        /// specified collection.
        /// </summary>
        /// <param name="requiredCollection">
        /// The collection of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityFilter"/> that requires specific component types from
        /// <paramref name="requiredCollection"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredCollection"/> does not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="requiredCollection"/> is <see langword="null"/>.
        /// </exception>
        public static EntityFilter Create(IEnumerable<ComponentType> requiredCollection)
        {
            return CreateInstance(requiredCollection.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityFilter"/> that requires, includes, and excludes specific
        /// component types from the specified collections.
        /// </summary>
        /// <param name="requiredCollection">
        /// The collection of required component types.
        /// </param>
        /// <param name="includedCollection">
        /// The collection of included component types.
        /// </param>
        /// <param name="excludedCollection">
        /// The collection of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityFilter"/> that requires, includes, and excludes specific component
        /// types from <paramref name="requiredCollection"/>, <paramref name="includedCollection"/>, and
        /// <paramref name="excludedCollection"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredCollection"/>, <paramref name="includedCollection"/>, and
        /// <paramref name="excludedCollection"/> do not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="requiredCollection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="includedCollection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="excludedCollection"/> is <see langword="null"/>.
        /// </exception>
        public static EntityFilter Create(IEnumerable<ComponentType> requiredCollection,
                                          IEnumerable<ComponentType> includedCollection,
                                          IEnumerable<ComponentType> excludedCollection)
        {
            return CreateInstance(requiredCollection.ToArray(),
                                  includedCollection.ToArray(),
                                  excludedCollection.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityFilter"/> that requires specific component types from the
        /// specified span.
        /// </summary>
        /// <param name="requiredSpan">
        /// The span of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityFilter"/> that requires specific component types from
        /// <paramref name="requiredSpan"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredSpan"/> does not contain any component types.
        /// </returns>
        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredSpan)
        {
            return CreateInstance(requiredSpan.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityFilter"/> that requires, includes, and excludes specific
        /// component types from the specified spans.
        /// </summary>
        /// <param name="requiredSpan">
        /// The span of required component types.
        /// </param>
        /// <param name="includedSpan">
        /// The span of included component types.
        /// </param>
        /// <param name="excludedSpan">
        /// The span of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityFilter"/> that requires, includes, and excludes specific component
        /// types from <paramref name="requiredSpan"/>, <paramref name="includedSpan"/>, and
        /// <paramref name="excludedSpan"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredSpan"/>, <paramref name="includedSpan"/>, and
        /// <paramref name="excludedSpan"/> do not contain any component types.
        /// </returns>
        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredSpan,
                                          ReadOnlySpan<ComponentType> includedSpan,
                                          ReadOnlySpan<ComponentType> excludedSpan)
        {
            return CreateInstance(requiredSpan.ToArray(),
                                  includedSpan.ToArray(),
                                  excludedSpan.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct
        /// <see cref="EntityFilter"/> instances that require, include, and exclude specific
        /// component types from a variety of inputs.
        /// </summary>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct <see cref="EntityFilter"/>
        /// instances that require, include, and exclude specific component types from a variety of
        /// inputs.
        /// </returns>
        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that requires specific component types from the specified
        /// array.
        /// </summary>
        /// <param name="array">
        /// The array of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that requires specific component types from <paramref name="array"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Require(ComponentType[] array)
        {
            return new Builder().Require(array);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that requires specific component types from the specified
        /// collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that requires specific component types from <paramref name="collection"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Require(IEnumerable<ComponentType> collection)
        {
            return new Builder().Require(collection);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that requires specific component types from the specified
        /// span.
        /// </summary>
        /// <param name="span">
        /// The span of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that requires specific component types from <paramref name="span"/>, if any.
        /// </returns>
        public static Builder Require(ReadOnlySpan<ComponentType> span)
        {
            return new Builder().Require(span);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that includes specific component types from the specified
        /// array.
        /// </summary>
        /// <param name="array">
        /// The array of included component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that includes specific component types from <paramref name="array"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Include(ComponentType[] array)
        {
            return new Builder().Include(array);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that includes specific component types from the specified
        /// collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of included component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that includes specific component types from <paramref name="collection"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Include(IEnumerable<ComponentType> collection)
        {
            return new Builder().Include(collection);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that includes specific component types from the specified
        /// span.
        /// </summary>
        /// <param name="span">
        /// The span of included component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that includes specific component types from <paramref name="span"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="span"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Include(ReadOnlySpan<ComponentType> span)
        {
            return new Builder().Include(span);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that excludes specific component types from the specified
        /// array.
        /// </summary>
        /// <param name="array">
        /// The array of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that excludes specific component types from <paramref name="array"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Exclude(ComponentType[] array)
        {
            return new Builder().Exclude(array);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that excludes specific component types from the specified
        /// collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that excludes specific component types from <paramref name="collection"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Exclude(IEnumerable<ComponentType> collection)
        {
            return new Builder().Exclude(collection);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityFilter"/> that excludes specific component types from the specified
        /// span.
        /// </summary>
        /// <param name="span">
        /// The span of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityFilter"/>
        /// that excludes specific component types from <paramref name="span"/>, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="span"/> is <see langword="null"/>.
        /// </exception>
        public static Builder Exclude(ReadOnlySpan<ComponentType> span)
        {
            return new Builder().Exclude(span);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityFilter"/> requires the specified component type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityFilter"/> requires
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Requires(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(RequiredComponentBitmask, componentType.TypeId);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityFilter"/> includes the specified component type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityFilter"/> includes
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Includes(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(IncludedComponentBitmask, componentType.TypeId);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityFilter"/> excludes the specified component type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityFilter"/> excludes
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Excludes(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(ExcludedComponentBitmask, componentType.TypeId);
        }

        /// <summary>
        /// Determines whether the specified archetype meets the criteria defined by the
        /// <see cref="EntityFilter"/>.
        /// </summary>
        /// <param name="archetype">
        /// The archetype to compare against the criteria defined by the <see cref="EntityFilter"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="archetype"/> meets the criteria defined by the
        /// <see cref="EntityFilter"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Matches(EntityArchetype archetype)
        {
            if (archetype is null)
            {
                return false;
            }

            ReadOnlySpan<int> bitmask = archetype.ComponentBitmask;

            return BitmaskOperations.Requires(RequiredComponentBitmask, bitmask)
                && BitmaskOperations.Includes(IncludedComponentBitmask, bitmask)
                && BitmaskOperations.Excludes(ExcludedComponentBitmask, bitmask);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct
        /// <see cref="EntityFilter"/> instances based on the criteria defined by the
        /// <see cref="EntityFilter"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct <see cref="EntityFilter"/>
        /// instances based on the criteria defined by the <see cref="EntityFilter"/>.
        /// </returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals"/>
        public bool Equals(EntityFilter? other)
        {
            return ReferenceEquals(other, this)
                || other is not null
                && RequiredComponentBitmask.SequenceEqual(other.RequiredComponentBitmask)
                && IncludedComponentBitmask.SequenceEqual(other.IncludedComponentBitmask)
                && ExcludedComponentBitmask.SequenceEqual(other.ExcludedComponentBitmask);
        }

        /// <inheritdoc cref="object.Equals"/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityFilter);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(BitmaskOperations.GetHashCode(RequiredComponentBitmask),
                                    BitmaskOperations.GetHashCode(IncludedComponentBitmask),
                                    BitmaskOperations.GetHashCode(ExcludedComponentBitmask));
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            string requiredComponentTypes = string.Join(", ", (object[])m_requiredComponentTypes);
            string includedComponentTypes = string.Join(", ", (object[])m_includedComponentTypes);
            string excludedComponentTypes = string.Join(", ", (object[])m_excludedComponentTypes);

            return $"EntityFilter {{ RequiredComponentTypes = [{requiredComponentTypes}]" +
                                  $" IncludedComponentTypes = [{includedComponentTypes}]" +
                                  $" ExcludedComponentTypes = [{excludedComponentTypes}] }}";
        }

        private static EntityFilter CreateInstance(ComponentType[] requiredComponentTypes)
        {
            if (TryBuild(ref requiredComponentTypes, out int[] requiredComponentBitmask))
            {
                return new EntityFilter(requiredComponentTypes, requiredComponentBitmask);
            }

            return s_universal;
        }

        private static EntityFilter CreateInstance(ComponentType[] requiredComponentTypes,
                                                   ComponentType[] includedComponentTypes,
                                                   ComponentType[] excludedComponentTypes)
        {
            if (TryBuild(ref requiredComponentTypes, out int[] requiredComponentBitmask) |
                TryBuild(ref includedComponentTypes, out int[] includedComponentBitmask) |
                TryBuild(ref excludedComponentTypes, out int[] excludedComponentBitmask))
            {
                return new EntityFilter(requiredComponentTypes, requiredComponentBitmask,
                                        includedComponentTypes, includedComponentBitmask,
                                        excludedComponentTypes, excludedComponentBitmask);
            }

            return s_universal;
        }

        private static bool TryBuild(ref ComponentType[] componentTypes, out int[] componentBitmask)
        {
            ComponentType[] localComponentTypes = componentTypes;

            if (localComponentTypes.Length == 0)
            {
                componentBitmask = Array.Empty<int>();
                return false;
            }

            Array.Sort(localComponentTypes);

            ComponentType? previousComponentType = localComponentTypes[^1];

            if (previousComponentType == null)
            {
                componentTypes = Array.Empty<ComponentType>();
                componentBitmask = Array.Empty<int>();
                return false;
            }

            int[] localComponentBitmask = new int[previousComponentType.TypeId + 32 >> 5];
            int count = 0;

            previousComponentType = null;

            foreach (ComponentType? currentComponentType in localComponentTypes)
            {
                if (currentComponentType != previousComponentType)
                {
                    int typeId = currentComponentType.TypeId;

                    localComponentTypes[count++] = previousComponentType = currentComponentType;
                    localComponentBitmask[typeId >> 5] |= 1 << typeId;
                }
            }

            Array.Resize(ref localComponentTypes, count);
            componentTypes = localComponentTypes;
            componentBitmask = localComponentBitmask;
            return true;
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator =="/>
        public static bool operator ==(EntityFilter? left, EntityFilter? right)
        {
            return ReferenceEquals(left, right)
                || left is not null
                && right is not null
                && left.RequiredComponentBitmask.SequenceEqual(right.RequiredComponentBitmask)
                && left.IncludedComponentBitmask.SequenceEqual(right.IncludedComponentBitmask)
                && left.ExcludedComponentBitmask.SequenceEqual(right.ExcludedComponentBitmask);
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator !="/>
        public static bool operator !=(EntityFilter? left, EntityFilter? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Represents a builder that can be used to construct <see cref="EntityFilter"/> instances
        /// that require, include, and exclude specific component types from a variety of inputs.
        /// </summary>
        public sealed class Builder
        {
            private ComponentType[] m_requiredComponentTypes;
            private ComponentType[] m_includedComponentTypes;
            private ComponentType[] m_excludedComponentTypes;
            private int[] m_requiredComponentBitmask;
            private int[] m_includedComponentBitmask;
            private int[] m_excludedComponentBitmask;

            internal Builder()
            {
                Reset();
            }

            internal Builder(EntityFilter filter)
            {
                m_requiredComponentTypes = filter.m_requiredComponentTypes;
                m_includedComponentTypes = filter.m_includedComponentTypes;
                m_excludedComponentTypes = filter.m_excludedComponentTypes;
                m_requiredComponentBitmask = filter.m_requiredComponentBitmask;
                m_includedComponentBitmask = filter.m_includedComponentBitmask;
                m_excludedComponentBitmask = filter.m_excludedComponentBitmask;
            }

            /// <summary>
            /// Creates an <see cref="EntityFilter"/> that requires, includes, and excludes
            /// component types specified by the <see cref="Builder"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="EntityFilter"/> that requires, includes, and excludes component types
            /// specified by the <see cref="Builder"/>, or <see cref="Universal"/> if the
            /// <see cref="Builder"/> does not specify any component types.
            /// </returns>
            public EntityFilter Construct()
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

            /// <summary>
            /// Sets the <see cref="Builder"/> to its default state, which specifies a criteria
            /// identical to the one defined by <see cref="Universal"/>.
            /// </summary>
            [MemberNotNull(nameof(m_requiredComponentTypes), nameof(m_requiredComponentBitmask),
                           nameof(m_includedComponentTypes), nameof(m_includedComponentBitmask),
                           nameof(m_excludedComponentTypes), nameof(m_excludedComponentBitmask))]
            public void Reset()
            {
                m_requiredComponentTypes = Array.Empty<ComponentType>();
                m_includedComponentTypes = Array.Empty<ComponentType>();
                m_excludedComponentTypes = Array.Empty<ComponentType>();
                m_requiredComponentBitmask = Array.Empty<int>();
                m_includedComponentBitmask = Array.Empty<int>();
                m_excludedComponentBitmask = Array.Empty<int>();
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> requires specific component types from the
            /// specified array, if any.
            /// </summary>
            /// <param name="array">
            /// The array of required component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is <see langword="null"/>.
            /// </exception>
            public Builder Require(ComponentType[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                return BuildRequiredParameters(new ReadOnlySpan<ComponentType>(array).ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> requires specific component types from the
            /// specified collection, if any.
            /// </summary>
            /// <param name="collection">
            /// The collection of required component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="collection"/> is <see langword="null"/>.
            /// </exception>
            public Builder Require(IEnumerable<ComponentType> collection)
            {
                return BuildRequiredParameters(collection.ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> requires specific component types from the
            /// specified span, if any.
            /// </summary>
            /// <param name="span">
            /// The span of required component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            public Builder Require(ReadOnlySpan<ComponentType> span)
            {
                return BuildRequiredParameters(span.ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> includes specific component types from the
            /// specified array, if any.
            /// </summary>
            /// <param name="array">
            /// The array of included component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is <see langword="null"/>.
            /// </exception>
            public Builder Include(ComponentType[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                return BuildIncludedParameters(new ReadOnlySpan<ComponentType>(array).ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> includes specific component types from the
            /// specified collection, if any.
            /// </summary>
            /// <param name="collection">
            /// The collection of included component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="collection"/> is <see langword="null"/>.
            /// </exception>
            public Builder Include(IEnumerable<ComponentType> collection)
            {
                return BuildIncludedParameters(collection.ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> includes specific component types from the
            /// specified span, if any.
            /// </summary>
            /// <param name="span">
            /// The span of included component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            public Builder Include(ReadOnlySpan<ComponentType> span)
            {
                return BuildIncludedParameters(span.ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> excludes specific component types from the
            /// specified array, if any.
            /// </summary>
            /// <param name="array">
            /// The array of excluded component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is <see langword="null"/>.
            /// </exception>
            public Builder Exclude(ComponentType[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                return BuildExcludedParameters(new ReadOnlySpan<ComponentType>(array).ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> excludes specific component types from the
            /// specified collection, if any.
            /// </summary>
            /// <param name="collection">
            /// The collection of excluded component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="collection"/> is <see langword="null"/>.
            /// </exception>
            public Builder Exclude(IEnumerable<ComponentType> collection)
            {
                return BuildExcludedParameters(collection.ToArray());
            }

            /// <summary>
            /// Updates the criteria specified by the <see cref="Builder"/> such that the
            /// constructed <see cref="EntityFilter"/> excludes specific component types from the
            /// specified span, if any.
            /// </summary>
            /// <param name="span">
            /// The span of excluded component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/> which can be used to chain method calls.
            /// </returns>
            public Builder Exclude(ReadOnlySpan<ComponentType> span)
            {
                return BuildExcludedParameters(span.ToArray());
            }

            private Builder BuildRequiredParameters(ComponentType[] componentTypes)
            {
                TryBuild(ref componentTypes, out int[] componentBitmask);

                m_requiredComponentTypes = componentTypes;
                m_requiredComponentBitmask = componentBitmask;
                return this;
            }

            private Builder BuildIncludedParameters(ComponentType[] componentTypes)
            {
                TryBuild(ref componentTypes, out int[] componentBitmask);

                m_includedComponentTypes = componentTypes;
                m_includedComponentBitmask = componentBitmask;
                return this;
            }

            private Builder BuildExcludedParameters(ComponentType[] componentTypes)
            {
                TryBuild(ref componentTypes, out int[] componentBitmask);

                m_excludedComponentTypes = componentTypes;
                m_excludedComponentBitmask = componentBitmask;
                return this;
            }
        }
    }
}
