// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

namespace Monophyll.Entities
{
    /// <summary>
    /// Specifies the categories of component types.
    /// </summary>
    public enum ComponentTypeCategory
    {
        /// <summary>
        /// Specifies that the component type is an unknown type.
        /// </summary>
        Unknown,
        /// <summary>
        /// Specifies that the component type is a reference type or a value type that contains
        /// reference type fields.
        /// </summary>
        Managed,
        /// <summary>
        /// Specifies that the component type is a value type with no reference type fields.
        /// </summary>
        Unmanaged,
        /// <summary>
        /// Specifies that the component type is a value type with no fields.
        /// </summary>
        Tag
    }
}
