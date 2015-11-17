using System;
using System.Collections.Generic;

namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    /// Class MapHierarchy.
    /// </summary>
    internal class MapHierarchy : IMapHierarchy
    {
        #region Private variables

        #endregion Private variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MapHierarchy"/> class.
        /// </summary>
        public MapHierarchy()
        {
            TypeChain = new List<Type>();
        }

        #endregion Constructor

        #region Internal Properties

        /// <summary>
        /// Gets the type chain.
        /// </summary>
        /// <value>The type chain.</value>
        internal List<Type> TypeChain { get; }

        #endregion Internal Properties

        #region Public methods

        /// <summary>
        /// Includes this instance.
        /// </summary>
        /// <typeparam name="Type">The type of the type.</typeparam>
        /// <returns>IMapHierarchy.</returns>
        public IMapHierarchy Include<Type>()
        {
            TypeChain.Add(typeof(Type));
            return this;
        }

        #endregion Public methods
    }
}