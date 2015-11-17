namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    ///     Interface IMapHierarchy
    /// </summary>
    public interface IMapHierarchy
    {
        /// <summary>
        ///     Includes this instance.
        /// </summary>
        /// <typeparam name="Type">The type of the type.</typeparam>
        /// <returns>IMapHierarchy.</returns>
        IMapHierarchy Include<Type>();
    }
}