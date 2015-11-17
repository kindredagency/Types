using System;

namespace Framework.AssetLibrary.Types
{
    /// <summary>
    /// Class CollectionDetails.
    /// </summary>
    public class CollectionDetails
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is collection.
        /// </summary>
        /// <value><c>true</c> if this instance is collection; otherwise, <c>false</c>.</value>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Gets or sets the type of the collection.
        /// </summary>
        /// <value>The type of the collection.</value>
        public CollectionType CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the type of the collection element.
        /// </summary>
        /// <value>The type of the collection element.</value>
        public Type CollectionElementType { get; set; }

    }
}
