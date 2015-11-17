namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    ///     Class MapperFactory.
    /// </summary>
    public class MapperFactory
    {
        #region Private variables

        private readonly IMapperContext _mapperContext;

        #endregion Private variables

        #region Public methods

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <returns>IMapperContext.</returns>
        public IMapperContext GetContext()
        {
            return _mapperContext;
        }

        #endregion Public methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperFactory"/> class.
        /// </summary>
        public MapperFactory()
        {
            _mapperContext = new MapperContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperFactory"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MapperFactory(IMapperContext context)
        {
            _mapperContext = context;
        }

        #endregion Constructors
    }
}