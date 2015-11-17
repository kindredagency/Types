using System;
using System.Collections.Generic;

namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    ///     Interface IMapperContext
    /// </summary>
    public interface IMapperContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether [allow only included types].
        /// </summary>
        /// <value><c>true</c> if [allow only included types]; otherwise, <c>false</c>.</value>
        bool AllowOnlyIncludedTypes { get; set; }

        /// <summary>
        /// Adds the map.
        /// </summary>
        /// <typeparam name="TypeFrom">The type of the type from.</typeparam>
        /// <typeparam name="TypeTo">The type of the type to.</typeparam>
        /// <returns>IMap.</returns>
        IMap AddMap<TypeFrom, TypeTo>()
            where TypeFrom : class
            where TypeTo : class;


        /// <summary>
        /// Maps the specified from object.
        /// </summary>
        /// <typeparam name="FromType">The type of from type.</typeparam>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="fromObject">From object.</param>
        /// <returns>ToType.</returns>
        ToType Map<FromType, ToType>(FromType fromObject);

        /// <summary>
        /// Maps the collection.
        /// </summary>
        /// <typeparam name="FromType">The type of from type.</typeparam>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="fromCollection">From collection.</param>
        /// <returns>IEnumerable&lt;ToType&gt;.</returns>
        IEnumerable<ToType> MapCollection<FromType, ToType>(IEnumerable<FromType> fromCollection);

        /// <summary>
        /// Adds the hierarchy.
        /// </summary>
        /// <typeparam name="RootType">The type of the root type.</typeparam>
        /// <returns>IMapHierarchy.</returns>
        IMapHierarchy AddHierarchy<RootType>();

        /// <summary>
        /// Adds the interface to type map.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="actualType">The actual type.</param>
        void AddInterfaceToTypeMap(Type interfaceType, Type actualType);
    }
}