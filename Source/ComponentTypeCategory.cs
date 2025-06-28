// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

namespace Monophyll.Entities
{
    /// <summary>
    /// Specifies component type categories.
    /// </summary>
    public enum ComponentTypeCategory
    {
        /// <summary>
        /// Specifies that the component type is unknown.
        /// </summary>
        None,
        /// <summary>
        /// Specifies that the component type is a reference type or a value type that
        /// contains reference type instance fields.
        /// </summary>
        Managed,
        /// <summary>
        /// Specifies that the component type is a value type with unmanaged instance fields.
        /// </summary>
        Unmanaged,
        /// <summary>
        /// Specifies that the component type is a value type with no instance fields.
        /// </summary>
        Tag
    }
}
