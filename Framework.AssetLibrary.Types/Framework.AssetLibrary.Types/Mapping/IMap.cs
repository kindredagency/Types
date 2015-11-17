using System;
using System.ComponentModel;

namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    ///     Interface IMap
    /// </summary>
    public interface IMap
    {
        /// <summary>
        ///     Gets or sets from type.
        /// </summary>
        /// <value>From type.</value>
        Type FromType { get; set; }
       
        /// <summary>
        ///     Gets or sets the type of the map.
        /// </summary>
        /// <value>The type of the map.</value>
        MapTypeDefinition MapType { get; set; }

        /// <summary>
        ///     Gets or sets to type.
        /// </summary>
        /// <value>To type.</value>
        Type ToType { get; set; }

        /// <summary>
        ///     Gets or sets the type from properties.
        /// </summary>
        /// <value>The type from properties.</value>
        PropertyDescriptorCollection TypeFromProperties { get; set; }

        /// <summary>
        ///     Gets or sets the type to properties.
        /// </summary>
        /// <value>The type to properties.</value>
        PropertyDescriptorCollection TypeToProperties { get; set; }
    }
}