using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    /// Class Map.
    /// </summary>
    internal class Map : IMap
    {
        #region Private variables

        public Dictionary<string, string> MapsDictionary { get; }

        #endregion Private variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <param name="toType">To type.</param>
        public Map(Type fromType, Type toType)
        {  
            MapType = MapTypeDefinition.Original;
            TypeFromProperties = TypeDescriptor.GetProperties(fromType);
            TypeToProperties = TypeDescriptor.GetProperties(toType);
            FromType = fromType;
            ToType = toType;
            MapsDictionary = new Dictionary<string, string>();
        }

        #endregion Constructor

        #region Public properties


        /// <summary>
        /// Gets or sets the type of the map.
        /// </summary>
        /// <value>The type of the map.</value>
        public MapTypeDefinition MapType { get; set; }

        /// <summary>
        /// Gets or sets the type from properties.
        /// </summary>
        /// <value>The type from properties.</value>
        public PropertyDescriptorCollection TypeFromProperties { get; set; }

        /// <summary>
        /// Gets or sets the type to properties.
        /// </summary>
        /// <value>The type to properties.</value>
        public PropertyDescriptorCollection TypeToProperties { get; set; }

        /// <summary>
        /// Gets or sets from type.
        /// </summary>
        /// <value>From type.</value>
        public Type FromType { get; set; }

        /// <summary>
        /// Gets or sets to type.
        /// </summary>
        /// <value>To type.</value>
        public Type ToType { get; set; }

        #endregion Public properties
    }
}