// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a predicate that determines whether archetypes satisfy a set of conditions in
    /// terms of required, included, and excluded component types.
    /// </summary>
    public sealed class EntityPredicate : IEquatable<EntityPredicate>
    {
        private static readonly EntityPredicate s_universal = new EntityPredicate();

        private readonly ComponentType[] m_requiredComponentTypes;
        private readonly ComponentType[] m_includedComponentTypes;
        private readonly ComponentType[] m_excludedComponentTypes;
        private readonly int[] m_requiredComponentBitmap;
        private readonly int[] m_includedComponentBitmap;
        private readonly int[] m_excludedComponentBitmap;

        private EntityPredicate()
        {
            m_requiredComponentTypes = Array.Empty<ComponentType>();
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBitmap = Array.Empty<int>();
            m_includedComponentBitmap = Array.Empty<int>();
            m_excludedComponentBitmap = Array.Empty<int>();
        }

        private EntityPredicate(ComponentType[] requiredComponentTypes, int[] requiredComponentBitmap)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBitmap = requiredComponentBitmap;
            m_includedComponentBitmap = Array.Empty<int>();
            m_excludedComponentBitmap = Array.Empty<int>();
        }

        private EntityPredicate(ComponentType[] requiredComponentTypes, int[] requiredComponentBitmap,
                                ComponentType[] includedComponentTypes, int[] includedComponentBitmap,
                                ComponentType[] excludedComponentTypes, int[] excludedComponentBitmap)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = includedComponentTypes;
            m_excludedComponentTypes = excludedComponentTypes;
            m_requiredComponentBitmap = requiredComponentBitmap;
            m_includedComponentBitmap = includedComponentBitmap;
            m_excludedComponentBitmap = excludedComponentBitmap;
        }

        /// <summary>
        /// Gets an <see cref="EntityPredicate"/> that is satisfiable over the set of all possible
        /// archetypes.
        /// </summary>
        /// <returns>
        /// An <see cref="EntityPredicate"/> that is satisfiable over the set of all possible
        /// archetypes.
        /// </returns>
        public static EntityPredicate Universal
        {
            get => s_universal;
        }

        /// <summary>
        /// Gets a read-only span of component types that archetypes must contain in order to
        /// satisfy the <see cref="EntityPredicate"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that archetypes must contain in order to satisfy the
        /// <see cref="EntityPredicate"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> RequiredComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_requiredComponentTypes);
        }

        /// <summary>
        /// Gets a read-only span of component types that archetypes must contain at least one
        /// instance of in order to satisfy the <see cref="EntityPredicate"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that, if not empty, archetypes must contain at least
        /// one instance of in order to satisfy the <see cref="EntityPredicate"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> IncludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_includedComponentTypes);
        }

        /// <summary>
        /// Gets a read-only span of component types that archetypes must not contain in order to
        /// satisfy the <see cref="EntityPredicate"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that archetypes must not contain in order to satisfy
        /// the <see cref="EntityPredicate"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> ExcludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_excludedComponentTypes);
        }

        /// <summary>
        /// Gets a read-only bitmap that compactly stores flags indicating whether the
        /// <see cref="EntityPredicate"/> requires a specific component type.
        /// </summary>
        /// <returns>
        /// A read-only bitmap that compactly stores flags indicating whether the
        /// <see cref="EntityPredicate"/> requires a specific component type.
        /// </returns>
        public ReadOnlySpan<int> RequiredComponentBitmap
        {
            get => new ReadOnlySpan<int>(m_requiredComponentBitmap);
        }

        /// <summary>
        /// Gets a read-only bitmap that compactly stores flags indicating whether the
        /// <see cref="EntityPredicate"/> includes a specific component type.
        /// </summary>
        /// <returns>
        /// A read-only bitmap that compactly stores flags indicating whether the
        /// <see cref="EntityPredicate"/> includes a specific component type.
        /// </returns>
        public ReadOnlySpan<int> IncludedComponentBitmap
        {
            get => new ReadOnlySpan<int>(m_includedComponentBitmap);
        }

        /// <summary>
        /// Gets a read-only bitmap that compactly stores flags indicating whether the
        /// <see cref="EntityPredicate"/> excludes a specific component type.
        /// </summary>
        /// <returns>
        /// A read-only bitmap that compactly stores flags indicating whether the
        /// <see cref="EntityPredicate"/> excludes a specific component type.
        /// </returns>
        public ReadOnlySpan<int> ExcludedComponentBitmap
        {
            get => new ReadOnlySpan<int>(m_excludedComponentBitmap);
        }

        /// <summary>
        /// Creates an <see cref="EntityPredicate"/> that requires specific component types from the
        /// specified array.
        /// </summary>
        /// <param name="requiredArray">
        /// The array of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityPredicate"/> that requires specific component types from
        /// <paramref name="requiredArray"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredArray"/> does not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="requiredArray"/> is <see langword="null"/>.
        /// </exception>
        public static EntityPredicate Create(ComponentType[] requiredArray)
        {
            ArgumentNullException.ThrowIfNull(requiredArray);
            return CreateInstance(new ReadOnlySpan<ComponentType>(requiredArray).ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityPredicate"/> that requires, includes, and excludes specific
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
        /// An <see cref="EntityPredicate"/> that requires, includes, and excludes specific
        /// component types from <paramref name="requiredArray"/>, <paramref name="includedArray"/>,
        /// and <paramref name="excludedArray"/>, or <see cref="Universal"/> if
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
        public static EntityPredicate Create(ComponentType[] requiredArray,
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
        /// Creates an <see cref="EntityPredicate"/> that requires specific component types from the
        /// specified collection.
        /// </summary>
        /// <param name="requiredCollection">
        /// The collection of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityPredicate"/> that requires specific component types from
        /// <paramref name="requiredCollection"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredCollection"/> does not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="requiredCollection"/> is <see langword="null"/>.
        /// </exception>
        public static EntityPredicate Create(IEnumerable<ComponentType> requiredCollection)
        {
            return CreateInstance(requiredCollection.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityPredicate"/> that requires, includes, and excludes specific
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
        /// An <see cref="EntityPredicate"/> that requires, includes, and excludes specific
        /// component types from <paramref name="requiredCollection"/>,
        /// <paramref name="includedCollection"/>, and <paramref name="excludedCollection"/>, or
        /// <see cref="Universal"/> if <paramref name="requiredCollection"/>,
        /// <paramref name="includedCollection"/>, and <paramref name="excludedCollection"/> do not
        /// contain any component types.
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
        public static EntityPredicate Create(IEnumerable<ComponentType> requiredCollection,
                                             IEnumerable<ComponentType> includedCollection,
                                             IEnumerable<ComponentType> excludedCollection)
        {
            return CreateInstance(requiredCollection.ToArray(),
                                  includedCollection.ToArray(),
                                  excludedCollection.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityPredicate"/> that requires specific component types from the
        /// specified span.
        /// </summary>
        /// <param name="requiredSpan">
        /// The span of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityPredicate"/> that requires specific component types from
        /// <paramref name="requiredSpan"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredSpan"/> does not contain any component types.
        /// </returns>
        public static EntityPredicate Create(ReadOnlySpan<ComponentType> requiredSpan)
        {
            return CreateInstance(requiredSpan.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityPredicate"/> that requires, includes, and excludes specific
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
        /// An <see cref="EntityPredicate"/> that requires, includes, and excludes specific
        /// component types from <paramref name="requiredSpan"/>, <paramref name="includedSpan"/>,
        /// and <paramref name="excludedSpan"/>, or <see cref="Universal"/> if
        /// <paramref name="requiredSpan"/>, <paramref name="includedSpan"/>, and
        /// <paramref name="excludedSpan"/> do not contain any component types.
        /// </returns>
        public static EntityPredicate Create(ReadOnlySpan<ComponentType> requiredSpan,
                                             ReadOnlySpan<ComponentType> includedSpan,
                                             ReadOnlySpan<ComponentType> excludedSpan)
        {
            return CreateInstance(requiredSpan.ToArray(),
                                  includedSpan.ToArray(),
                                  excludedSpan.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct
        /// <see cref="EntityPredicate"/> instances that require, include, and exclude specific
        /// component types from a variety of data sources.
        /// </summary>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct <see cref="EntityPredicate"/>
        /// instances that require, include, and exclude specific component types from a variety of
        /// data sources.
        /// </returns>
        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityPredicate"/> that requires specific component types from the specified
        /// array.
        /// </summary>
        /// <param name="array">
        /// The array of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that requires specific component types from the specified
        /// collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that requires specific component types from the specified
        /// span.
        /// </summary>
        /// <param name="span">
        /// The span of required component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
        /// that requires specific component types from <paramref name="span"/>, if any.
        /// </returns>
        public static Builder Require(ReadOnlySpan<ComponentType> span)
        {
            return new Builder().Require(span);
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct an
        /// <see cref="EntityPredicate"/> that includes specific component types from the specified
        /// array.
        /// </summary>
        /// <param name="array">
        /// The array of included component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that includes specific component types from the specified
        /// collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of included component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that includes specific component types from the specified
        /// span.
        /// </summary>
        /// <param name="span">
        /// The span of included component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that excludes specific component types from the specified
        /// array.
        /// </summary>
        /// <param name="array">
        /// The array of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that excludes specific component types from the specified
        /// collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// <see cref="EntityPredicate"/> that excludes specific component types from the specified
        /// span.
        /// </summary>
        /// <param name="span">
        /// The span of excluded component types.
        /// </param>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct an <see cref="EntityPredicate"/>
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
        /// Determines whether the <see cref="EntityPredicate"/> requires the specified component
        /// type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityPredicate"/> requires
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Requires(ComponentType componentType)
        {
            return componentType != null
                && BitmapOperations.Test(RequiredComponentBitmap, componentType.Index);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityPredicate"/> includes the specified component
        /// type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityPredicate"/> includes
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Includes(ComponentType componentType)
        {
            return componentType != null
                && BitmapOperations.Test(IncludedComponentBitmap, componentType.Index);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityPredicate"/> excludes the specified component type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityPredicate"/> excludes
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Excludes(ComponentType componentType)
        {
            return componentType != null
                && BitmapOperations.Test(ExcludedComponentBitmap, componentType.Index);
        }

        /// <summary>
        /// Determines whether the specified archetype satisfies the <see cref="EntityPredicate"/>.
        /// </summary>
        /// <param name="archetype">
        /// The archetype to compare against the set of conditions defined by the
        /// <see cref="EntityPredicate"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="archetype"/> satisfies the
        /// <see cref="EntityPredicate"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Test(EntityArchetype archetype)
        {
            if (archetype is null)
            {
                return false;
            }

            // Compare against the required component bitmap.
            ReadOnlySpan<int> targetComponentBitmap = archetype.ComponentBitmap;
            int[] sourceComponentBitmap = m_requiredComponentBitmap;
            int length = sourceComponentBitmap.Length;

            if (length > targetComponentBitmap.Length)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if ((sourceComponentBitmap[i] & ~targetComponentBitmap[i]) != 0)
                {
                    return false;
                }
            }

            // Compare against the included component bitmap.
            sourceComponentBitmap = m_includedComponentBitmap;
            length = sourceComponentBitmap.Length;

            if (length > 0)
            {
                if (length > targetComponentBitmap.Length)
                {
                    length = targetComponentBitmap.Length;
                }

                int i = 0;

                while (true)
                {
                    if (i == length)
                    {
                        return false;
                    }

                    if ((sourceComponentBitmap[i] & targetComponentBitmap[i]) != 0)
                    {
                        break;
                    }

                    i++;
                }
            }

            // Compare against the excluded component bitmap.
            sourceComponentBitmap = m_excludedComponentBitmap;
            length = Math.Min(sourceComponentBitmap.Length, targetComponentBitmap.Length);

            for (int i = 0; i < length; i++)
            {
                if ((sourceComponentBitmap[i] & targetComponentBitmap[i]) != 0)
                {
                    return false;
                }
            }

            // Return true after passing all comparisons.
            return true;
        }

        /// <summary>
        /// Creates an <see cref="Builder"/> that can be used to construct
        /// <see cref="EntityPredicate"/> instances based on the criteria defined by the
        /// <see cref="EntityPredicate"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Builder"/> that can be used to construct <see cref="EntityPredicate"/>
        /// instances based on the criteria defined by the <see cref="EntityPredicate"/>.
        /// </returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals"/>
        public bool Equals(EntityPredicate? other)
        {
            return ReferenceEquals(this, other)
                || other is not null
                && RequiredComponentBitmap.SequenceEqual(other.RequiredComponentBitmap)
                && IncludedComponentBitmap.SequenceEqual(other.IncludedComponentBitmap)
                && ExcludedComponentBitmap.SequenceEqual(other.ExcludedComponentBitmap);
        }

        /// <inheritdoc cref="object.Equals"/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityPredicate);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(BitmapOperations.GetHashCode(RequiredComponentBitmap),
                                    BitmapOperations.GetHashCode(IncludedComponentBitmap),
                                    BitmapOperations.GetHashCode(ExcludedComponentBitmap));
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            string requiredComponentTypes = string.Join(", ", (object[])m_requiredComponentTypes);
            string includedComponentTypes = string.Join(", ", (object[])m_includedComponentTypes);
            string excludedComponentTypes = string.Join(", ", (object[])m_excludedComponentTypes);

            return $"EntityPredicate {{ RequiredComponentTypes = [{requiredComponentTypes}]" +
                                     $" IncludedComponentTypes = [{includedComponentTypes}]" +
                                     $" ExcludedComponentTypes = [{excludedComponentTypes}] }}";
        }

        private static EntityPredicate CreateInstance(ComponentType[] requiredComponentTypes)
        {
            if (TryBuild(ref requiredComponentTypes, out int[] requiredComponentBitmap))
            {
                return new EntityPredicate(requiredComponentTypes, requiredComponentBitmap);
            }

            return s_universal;
        }

        private static EntityPredicate CreateInstance(ComponentType[] requiredComponentTypes,
                                                      ComponentType[] includedComponentTypes,
                                                      ComponentType[] excludedComponentTypes)
        {
            if (TryBuild(ref requiredComponentTypes, out int[] requiredComponentBitmap) |
                TryBuild(ref includedComponentTypes, out int[] includedComponentBitmap) |
                TryBuild(ref excludedComponentTypes, out int[] excludedComponentBitmap))
            {
                return new EntityPredicate(requiredComponentTypes, requiredComponentBitmap,
                                           includedComponentTypes, includedComponentBitmap,
                                           excludedComponentTypes, excludedComponentBitmap);
            }

            return s_universal;
        }

        private static bool TryBuild(ref ComponentType[] componentTypes, out int[] componentBitmap)
        {
            ComponentType[] localComponentTypes = componentTypes;

            if (localComponentTypes.Length == 0)
            {
                componentBitmap = Array.Empty<int>();
                return false;
            }

            Array.Sort(localComponentTypes);

            ComponentType? previousComponentType = localComponentTypes[^1];

            if (previousComponentType == null)
            {
                componentTypes = Array.Empty<ComponentType>();
                componentBitmap = Array.Empty<int>();
                return false;
            }

            int[] localComponentBitmap = componentBitmap = new int[previousComponentType.Index + 32 >> 5];
            int count = 0;

            previousComponentType = null;

            for (int i = 0; i < localComponentTypes.Length; i++)
            {
                ComponentType? currentComponentType = localComponentTypes[i];

                if (currentComponentType != previousComponentType)
                {
                    int componentTypeIndex = currentComponentType.Index;

                    localComponentTypes[count++] = previousComponentType = currentComponentType;
                    localComponentBitmap[componentTypeIndex >> 5] |= 1 << componentTypeIndex;
                }
            }

            Array.Resize(ref componentTypes, count);
            return true;
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator =="/>
        public static bool operator ==(EntityPredicate? left, EntityPredicate? right)
        {
            return ReferenceEquals(left, right)
                || left is not null
                && right is not null
                && left.RequiredComponentBitmap.SequenceEqual(right.RequiredComponentBitmap)
                && left.IncludedComponentBitmap.SequenceEqual(right.IncludedComponentBitmap)
                && left.ExcludedComponentBitmap.SequenceEqual(right.ExcludedComponentBitmap);
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator !="/>
        public static bool operator !=(EntityPredicate? left, EntityPredicate? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Represents a builder that can be used to construct <see cref="EntityPredicate"/>
        /// instances that require, include, and exclude specific component types from a variety of
        /// data sources.
        /// </summary>
        public sealed class Builder
        {
            private ComponentType[] m_requiredComponentTypes;
            private ComponentType[] m_includedComponentTypes;
            private ComponentType[] m_excludedComponentTypes;
            private int[] m_requiredComponentBitmap;
            private int[] m_includedComponentBitmap;
            private int[] m_excludedComponentBitmap;

            internal Builder()
            {
                Reset();
            }

            internal Builder(EntityPredicate predicate)
            {
                m_requiredComponentTypes = predicate.m_requiredComponentTypes;
                m_includedComponentTypes = predicate.m_includedComponentTypes;
                m_excludedComponentTypes = predicate.m_excludedComponentTypes;
                m_requiredComponentBitmap = predicate.m_requiredComponentBitmap;
                m_includedComponentBitmap = predicate.m_includedComponentBitmap;
                m_excludedComponentBitmap = predicate.m_excludedComponentBitmap;
            }

            /// <summary>
            /// Creates an <see cref="EntityPredicate"/> that requires, includes, and excludes
            /// component types specified by the <see cref="Builder"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="EntityPredicate"/> that requires, includes, and excludes component
            /// types specified by the <see cref="Builder"/>, or <see cref="Universal"/> if the
            /// <see cref="Builder"/> does not specify any component types.
            /// </returns>
            public EntityPredicate ToPredicate()
            {
                if (m_requiredComponentTypes.Length > 0 ||
                    m_includedComponentTypes.Length > 0 ||
                    m_excludedComponentTypes.Length > 0)
                {
                    return new EntityPredicate(m_requiredComponentTypes, m_requiredComponentBitmap,
                                               m_includedComponentTypes, m_includedComponentBitmap,
                                               m_excludedComponentTypes, m_excludedComponentBitmap);
                }

                return s_universal;
            }

            /// <summary>
            /// Sets the <see cref="Builder"/> to its default state, which specifies no required,
            /// included, or excluded component types.
            /// </summary>
            [MemberNotNull(nameof(m_requiredComponentTypes), nameof(m_requiredComponentBitmap),
                           nameof(m_includedComponentTypes), nameof(m_includedComponentBitmap),
                           nameof(m_excludedComponentTypes), nameof(m_excludedComponentBitmap))]
            public void Reset()
            {
                m_requiredComponentTypes = Array.Empty<ComponentType>();
                m_includedComponentTypes = Array.Empty<ComponentType>();
                m_excludedComponentTypes = Array.Empty<ComponentType>();
                m_requiredComponentBitmap = Array.Empty<int>();
                m_includedComponentBitmap = Array.Empty<int>();
                m_excludedComponentBitmap = Array.Empty<int>();
            }

            /// <summary>
            /// Sets the required component types specified by the <see cref="Builder"/> to the
            /// component types in the specified array, if any.
            /// </summary>
            /// <param name="array">
            /// The array of required component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is <see langword="null"/>.
            /// </exception>
            public Builder Require(ComponentType[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                return BuildRequiredMembers(new ReadOnlySpan<ComponentType>(array).ToArray());
            }

            /// <summary>
            /// Sets the required component types specified by the <see cref="Builder"/> to the
            /// component types in the specified collection, if any.
            /// </summary>
            /// <param name="collection">
            /// The collection of required component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="collection"/> is <see langword="null"/>.
            /// </exception>
            public Builder Require(IEnumerable<ComponentType> collection)
            {
                return BuildRequiredMembers(collection.ToArray());
            }

            /// <summary>
            /// Sets the required component types specified by the <see cref="Builder"/> to the
            /// component types in the specified span, if any.
            /// </summary>
            /// <param name="span">
            /// The span of required component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            public Builder Require(ReadOnlySpan<ComponentType> span)
            {
                return BuildRequiredMembers(span.ToArray());
            }

            /// <summary>
            /// Sets the included component types specified by the <see cref="Builder"/> to the
            /// component types in the specified array, if any.
            /// </summary>
            /// <param name="array">
            /// The array of included component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is <see langword="null"/>.
            /// </exception>
            public Builder Include(ComponentType[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                return BuildIncludedMembers(new ReadOnlySpan<ComponentType>(array).ToArray());
            }

            /// <summary>
            /// Sets the included component types specified by the <see cref="Builder"/> to the
            /// component types in the specified collection, if any.
            /// </summary>
            /// <param name="collection">
            /// The collection of included component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="collection"/> is <see langword="null"/>.
            /// </exception>
            public Builder Include(IEnumerable<ComponentType> collection)
            {
                return BuildIncludedMembers(collection.ToArray());
            }

            /// <summary>
            /// Sets the included component types specified by the <see cref="Builder"/> to the
            /// component types in the specified span, if any.
            /// </summary>
            /// <param name="span">
            /// The span of included component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            public Builder Include(ReadOnlySpan<ComponentType> span)
            {
                return BuildIncludedMembers(span.ToArray());
            }

            /// <summary>
            /// Sets the excluded component types specified by the <see cref="Builder"/> to the
            /// component types in the specified array, if any.
            /// </summary>
            /// <param name="array">
            /// The array of excluded component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is <see langword="null"/>.
            /// </exception>
            public Builder Exclude(ComponentType[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                return BuildExcludedMembers(new ReadOnlySpan<ComponentType>(array).ToArray());
            }

            /// <summary>
            /// Sets the excluded component types specified by the <see cref="Builder"/> to the
            /// component types in the specified collection, if any.
            /// </summary>
            /// <param name="collection">
            /// The collection of excluded component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="collection"/> is <see langword="null"/>.
            /// </exception>
            public Builder Exclude(IEnumerable<ComponentType> collection)
            {
                return BuildExcludedMembers(collection.ToArray());
            }

            /// <summary>
            /// Sets the excluded component types specified by the <see cref="Builder"/> to the
            /// component types in the specified span, if any.
            /// </summary>
            /// <param name="span">
            /// The span of excluded component types.
            /// </param>
            /// <returns>
            /// The <see cref="Builder"/>, which can be used to chain method calls.
            /// </returns>
            public Builder Exclude(ReadOnlySpan<ComponentType> span)
            {
                return BuildExcludedMembers(span.ToArray());
            }

            private Builder BuildRequiredMembers(ComponentType[] componentTypes)
            {
                TryBuild(ref componentTypes, out int[] componentBitmap);

                m_requiredComponentTypes = componentTypes;
                m_requiredComponentBitmap = componentBitmap;
                return this;
            }

            private Builder BuildIncludedMembers(ComponentType[] componentTypes)
            {
                TryBuild(ref componentTypes, out int[] componentBitmap);

                m_includedComponentTypes = componentTypes;
                m_includedComponentBitmap = componentBitmap;
                return this;
            }

            private Builder BuildExcludedMembers(ComponentType[] componentTypes)
            {
                TryBuild(ref componentTypes, out int[] componentBitmap);

                m_excludedComponentTypes = componentTypes;
                m_excludedComponentBitmap = componentBitmap;
                return this;
            }
        }
    }
}
