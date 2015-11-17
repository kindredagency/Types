using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Framework.AssetLibrary.Caching;
using System.ComponentModel;

namespace Framework.AssetLibrary.Types.Mapping
{
    /// <summary>
    ///     Class MapperContext.
    /// </summary>
    internal class MapperContext : IMapperContext
    {
        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapperContext" /> class.
        /// </summary>
        public MapperContext()
        {
            AllowOnlyIncludedTypes = true;
            _methodInfoForMapMethod = (typeof(IMapperContext)).GetMethod("Map");
            _methodInfoForMapCollectionMethod = (typeof(IMapperContext)).GetMethod("MapCollection");
        }

        #endregion Constructor

        #region Private Global variables

        //Stack to stop cyclical ref stack over flow. Its a stack of KEY / VALUE pairs where, the "KEY" is the base type and the "VALUE" is an instance of the mapped destination type.
        private readonly Stack<KeyValuePair<Type, object>> _activeExecutionChain = new Stack<KeyValuePair<Type, object>>();

        //Maps between object types
        private readonly Dictionary<KeyValuePair<Type, Type>, Map> _mapsDictionary =  new Dictionary<KeyValuePair<Type, Type>, Map>();

        //Maps of Hierarchy
        private readonly Dictionary<Type, MapHierarchy> _mapsHierarchyDictionary = new Dictionary<Type, MapHierarchy>();

        //Interface to type map
        private readonly Dictionary<Type, Type> _interfaceToTypeMap = new Dictionary<Type, Type>();

        //Flag to all allow only specified types

        //Hold the ref for reflection for the Map method
        private readonly MethodInfo _methodInfoForMapMethod;

        //Hold the ref for reflection for the Map method
        private readonly MethodInfo _methodInfoForMapCollectionMethod;

        #endregion Private Global variables

        #region IMapperContext Implementations

        /// <summary>
        ///     Gets or sets a value indicating whether [allow only included types].
        /// </summary>
        /// <value><c>true</c> if [allow only included types]; otherwise, <c>false</c>.</value>
        public bool AllowOnlyIncludedTypes { get; set; }

        /// <summary>
        /// Adds the map.
        /// </summary>
        /// <typeparam name="TypeFrom">The type of the type from.</typeparam>
        /// <typeparam name="TypeTo">The type of the type to.</typeparam>
        /// <returns>IMap.</returns>
        public IMap AddMap<TypeFrom, TypeTo>()
            where TypeFrom : class
            where TypeTo : class
        {
            var map = new Map(typeof(TypeFrom), typeof(TypeTo));
            _mapsDictionary.Add(new KeyValuePair<Type, Type>(typeof(TypeFrom), typeof(TypeTo)), map);
            return map;
        }

        /// <summary>
        /// Adds the interface to type map.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="actualType">The actual type.</param>
        public void AddInterfaceToTypeMap(Type interfaceType, Type actualType)
        {
            if (!_interfaceToTypeMap.ContainsKey(interfaceType))
            {
                _interfaceToTypeMap.Add(interfaceType, actualType);
            }
        }

        /// <summary>
        /// Adds the hierarchy.
        /// </summary>
        /// <typeparam name="RootType">The type of the root type.</typeparam>
        /// <returns>IMapHierarchy.</returns>
        public IMapHierarchy AddHierarchy<RootType>()
        {
            MapHierarchy mapHierarchy = null;

            if (_mapsHierarchyDictionary.TryGetValue(typeof (RootType), out mapHierarchy)) return mapHierarchy;

            mapHierarchy = new MapHierarchy();

            _mapsHierarchyDictionary.Add(typeof(RootType), mapHierarchy);

            return mapHierarchy;
        }

        /// <summary>
        /// Maps the specified from object.
        /// </summary>
        /// <typeparam name="FromType">The type of from type.</typeparam>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="fromObject">From object.</param>
        /// <returns>ToType.</returns>
        /// <exception cref="System.Exception"></exception>
        public ToType Map<FromType, ToType>(FromType fromObject)
        {
            //Check if its a framework type or Custom type.
            if (typeof(FromType).IsFrameworkType() && typeof(ToType).IsFrameworkType())
            {
                //These are normal fields not complex objects so convert directly.
                var mapObject = (ToType)ChangeType(fromObject, typeof(ToType));
                return mapObject;
            }
            else
            {
                //Make a key to get the mapping rule from the dictionary
                var key = new KeyValuePair<Type, Type>(typeof(FromType), typeof(ToType));

                Map mapObject = null;

                //Get mapping rule using the key
                if (_mapsDictionary.ContainsKey(key))
                {
                    mapObject = _mapsDictionary[key];
                }
                else
                {
                    throw new Exception(
                        $"No mapping rule defined for {typeof(FromType).Name} => {typeof(ToType).Name}");
                }

                //Define the destination object.
                var toObject = SynchronizeMembers<FromType, ToType>(fromObject, mapObject.TypeFromProperties, mapObject.TypeToProperties);            

                return toObject;
            }
        }

        /// <summary>
        /// Maps the collection.
        /// </summary>
        /// <typeparam name="FromType">The type of from type.</typeparam>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="fromCollection">From collection.</param>
        /// <returns>IEnumerable&lt;ToType&gt;.</returns>
        public IEnumerable<ToType> MapCollection<FromType, ToType>(IEnumerable<FromType> fromCollection)
        {
            var fromTypes = fromCollection as FromType[] ?? fromCollection.ToArray();

            var toCollection = new List<ToType>(fromTypes.Count());

            toCollection.AddRange(fromTypes.Select(fromObject => Map<FromType, ToType>(fromObject)));

            return toCollection.AsEnumerable();
        }

        #endregion IMapperContext Implementations

        #region Private methods

        /// <summary>
        /// Synchronizes the members.
        /// </summary>
        /// <typeparam name="FromType">The type of from type.</typeparam>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="objFrom">The object from.</param>
        /// <param name="fromProperties">From properties.</param>
        /// <param name="toProperties">To properties.</param>
        /// <returns>ToType.</returns>
        private ToType SynchronizeMembers<FromType, ToType>(FromType objFrom, PropertyDescriptorCollection fromProperties, PropertyDescriptorCollection toProperties)
        {
            var objTo = default(ToType);

            if (objFrom == null) return objTo;

            Type destinationType;

            if (_interfaceToTypeMap.TryGetValue(typeof(ToType), out destinationType))
            {
                objTo = (ToType)Activator.CreateInstance(destinationType);
            }
            else
            {
                objTo = Activator.CreateInstance<ToType>();
            }

            #region Sync properties

            if (fromProperties == null || fromProperties.Count <= 0) return objTo;

            foreach (PropertyDescriptor fromProperty in fromProperties)
            {
                if (IsConversionAllowed(typeof(FromType), fromProperty.PropertyType))
                {
                    if (toProperties[fromProperty.Name] != null)
                    {
                        PropertyDescriptor destinationProperty = toProperties[fromProperty.Name];

                        PerformMapping(ref objTo, objFrom, fromProperty, destinationProperty);
                    }
                }
            }

            #endregion Sync properties

            return objTo;
        }
       
        /// <summary>
        ///     Performs the mapping.
        /// </summary>
        /// <typeparam name="FromType">The type of from type.</typeparam>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="objTo">The object to.</param>
        /// <param name="objFrom">The object from.</param>
        /// <param name="fromProperty">From property.</param>
        /// <param name="destinationProperty">The destination property.</param>
        private void PerformMapping<FromType, ToType>(ref ToType objTo, FromType objFrom, PropertyDescriptor fromProperty, PropertyDescriptor destinationProperty)
        {
            if (objFrom != null)
            {
                object parentReference;

                if (!IsCyclical(fromProperty.PropertyType, out parentReference))
                {
                    CollectionDetails fromCollectionDetails = fromProperty.PropertyType.IsCollection();
                    CollectionDetails destinationCollectionDetails = destinationProperty.PropertyType.IsCollection();
                    
                    var isFromACollection = fromCollectionDetails.IsCollection;
                    CollectionType fromCollectionType = fromCollectionDetails.CollectionType;
                    Type fromCollectionElementType = fromCollectionDetails.CollectionElementType;

                    var isDestinationACollection = destinationCollectionDetails.IsCollection;
                    CollectionType destinationCollectionType = destinationCollectionDetails.CollectionType;
                    Type destinationCollectionElementType = destinationCollectionDetails.CollectionElementType;

                    if ((fromCollectionType != CollectionType.IEnumerable) && (fromCollectionType != CollectionType.IList) && (fromCollectionType != CollectionType.ICollection))
                    {
                        //Check if the types match
                        if (fromProperty.PropertyType.Name == destinationProperty.PropertyType.Name)
                        {
                            //If : Properties are lists
                            if ((!isFromACollection) && (!isDestinationACollection))
                            {
                                //Its finalized that it is a single object type
                                var val = objFrom != null ? fromProperty.GetValue(objFrom) : null;

                                destinationProperty.SetValue(objTo, ChangeType(val, fromProperty.PropertyType));
                            }
                            else if ((isFromACollection) && (isDestinationACollection))
                            {
                                //For Collections
                                if (fromCollectionType == CollectionType.Array)
                                {
                                    //If : Properties are collections
                                    if ((!isFromACollection) && (!isDestinationACollection))
                                    {
                                        //Not a collection
                                        //check if they are part of the .net framework and if they are both Custom types then start mapping them based on rules by calling the map method recursively
                                        if (!fromProperty.PropertyType.IsFrameworkType() && !destinationProperty.PropertyType.IsFrameworkType())
                                        {
                                            //Prepare the method as a generic method
                                            var methodInfoMapMethodGeneric = _methodInfoForMapMethod.MakeGenericMethod(fromProperty.PropertyType, destinationProperty.PropertyType);

                                            //Push the current executing object into the chain stack before making the recursive call.
                                            _activeExecutionChain.Push(new KeyValuePair<Type, object>(typeof(FromType), objTo));

                                            //Call "Map" method recursively, Invoke the method.
                                            if (objFrom != null)
                                                destinationProperty.SetValue(objTo, methodInfoMapMethodGeneric.Invoke(this, new[] { fromProperty.GetValue(objFrom) }));
                                            else
                                                destinationProperty.SetValue(objTo, null);

                                            //Pop the last element from the stack and return it to the pre execution state.
                                            _activeExecutionChain.Pop();
                                        }
                                    }
                                    else if ((isFromACollection) && (isDestinationACollection))
                                    {
                                        //For arrays

                                        //Prepare the method as a generic method
                                        var methodInfoMapCollectionMethodGeneric = _methodInfoForMapCollectionMethod.MakeGenericMethod(fromCollectionElementType, destinationCollectionElementType);

                                        //Cast the list to an enumerable to iterate over it
                                        var fromList = fromProperty.GetValue(objFrom) as IEnumerable;

                                        var fromListLength = fromList != null ? fromList.Cast<object>().ToArray().Length : 0;

                                        //The destinationList is not created yet so make it
                                        var destinationList = fromList != null ? Activator.CreateInstance(destinationProperty.PropertyType, fromListLength) as object[] : null;

                                        //Push the current executing object into the chain stack before making the recursive call.
                                        _activeExecutionChain.Push(new KeyValuePair<Type, object>(typeof(FromType),
                                            objTo));

                                        var convertedCollection = fromList != null ? methodInfoMapCollectionMethodGeneric.Invoke(this, new object[] { fromList }) as IList : null;

                                        //Its finalized that it is a array so iterate
                                        for (var count = 0; count < fromListLength; count++)
                                        {
                                            destinationList[count] = convertedCollection[count];
                                        }

                                        //Pop the last element from the stack and return it to the pre execution state.
                                        _activeExecutionChain.Pop();

                                        //Not that the destination list has been made set it to the parent object.
                                        destinationProperty.SetValue(objTo, destinationList);
                                    }
                                }
                                else
                                {
                                    //Prepare the method as a generic method
                                    var methodInfoMapCollectionMethodGeneric = _methodInfoForMapCollectionMethod.MakeGenericMethod(fromCollectionElementType, destinationCollectionElementType);

                                    //Cast the list to an enumerable to iterate over it
                                    var fromList = fromProperty.GetValue(objFrom) as IEnumerable;

                                    //What ever type of enumerable that comes this way we make it into a List<T> and then convert it back.

                                    //Find the generic type
                                    var type = destinationProperty.PropertyType.GetGenericArguments()[0];

                                    //Find the type of the generic list
                                    var listType = typeof(List<>);

                                    //Make it generic by adding generics
                                    var concreteType = listType.MakeGenericType(type);

                                    //Push the current executing object into the chain stack before making the recursive call.
                                    _activeExecutionChain.Push(new KeyValuePair<Type, object>(typeof(FromType), objTo));

                                    //Its finalized that it is a collection so convert it
                                    var destinationList = fromList != null  ? methodInfoMapCollectionMethodGeneric.Invoke(this, new object[] { fromList }) as IEnumerable : null;

                                    //Pop the last element from the stack and return it to the pre execution state.
                                    _activeExecutionChain.Pop();

                                    //Not that the destination list has been made set it to the parent object.
                                    //And cast it as a enumerable and send it out.
                                    destinationProperty.SetValue(objTo, destinationList);
                                }
                            }
                        }
                        else
                        {
                            //names and types don't match

                            //If : Properties are collections.
                            if ((!isFromACollection) && (!isDestinationACollection))
                            {
                                //Not a collection
                                //Check if they are part of the .net framework and if they are both Custom types then start mapping them based on rules by calling the map method recursively
                                if (!fromProperty.PropertyType.IsFrameworkType() && !destinationProperty.PropertyType.IsFrameworkType())
                                {
                                    //Prepare the method as a generic method
                                    var methodInfoMapMethodGeneric = _methodInfoForMapMethod.MakeGenericMethod(fromProperty.PropertyType, destinationProperty.PropertyType);

                                    //Push the current executing object into the chain stack before making the recursive call.
                                    _activeExecutionChain.Push(new KeyValuePair<Type, object>(typeof(FromType), objTo));

                                    //Call "Map" method recursively, Invoke the method.
                                    if (objFrom != null)
                                        destinationProperty.SetValue(objTo, methodInfoMapMethodGeneric.Invoke(this, new[] { fromProperty.GetValue(objFrom) }));
                                    else
                                        destinationProperty.SetValue(objTo, null);

                                    //Pop the last element from the stack and return it to the pre execution state.
                                    _activeExecutionChain.Pop();
                                }
                            }
                            else if ((isFromACollection) && (isDestinationACollection))
                            {
                                //For arrays

                                //Prepare the method as a generic method
                                var methodInfoMapCollectionMethodGeneric = _methodInfoForMapCollectionMethod.MakeGenericMethod(fromCollectionElementType, destinationCollectionElementType);

                                //Cast the list to an enumerable to iterate over it
                                var fromList = fromProperty.GetValue(objFrom) as IEnumerable;
                                var fromListLength = fromList?.Cast<object>().ToArray().Length ?? 0;

                                //The destinationList is not created yet so make it
                                var destinationList = fromList != null ? Activator.CreateInstance(destinationProperty.PropertyType, fromListLength) as object[] : null;

                                //Push the current executing object into the chain stack before making the recursive call.
                                _activeExecutionChain.Push(new KeyValuePair<Type, object>(typeof(FromType), objTo));

                                var convertedCollection = fromList != null  ? methodInfoMapCollectionMethodGeneric.Invoke(this, new object[] { fromList }) as IList : null;

                                for (var count = 0; count < fromListLength; count++)
                                {
                                    if (destinationList != null)
                                        if (convertedCollection != null)
                                            destinationList[count] = convertedCollection[count];
                                }

                                //Pop the last element from the stack and return it to the pre execution state.
                                _activeExecutionChain.Pop();

                                //Not that the destination list has been made set it to the parent object.
                                destinationProperty.SetValue(objTo, destinationList);
                            }
                        }
                    }
                }
                else
                {
                    //Set the parent object ref to sub property to form the object reference.
                    if (parentReference != null)
                    {
                        destinationProperty.SetValue(objTo, parentReference);
                    }
                }
            }
        }

        /// <summary>
        ///     Changes the type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="conversion">The conversion.</param>
        /// <returns>System.Object.</returns>
        private object ChangeType(object value, Type conversion)
        {
            if (value == null)
            {
                return null;
            }

            if (conversion.IsGenericType && conversion.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                conversion = Nullable.GetUnderlyingType(conversion);
            }

            return Convert.ChangeType(value, conversion);
        }

        /// <summary>
        ///     Determines whether [is conversion allowed] [the specified root type].
        /// </summary>
        /// <param name="rootType">Type of the root.</param>
        /// <param name="includedType">Type of the included.</param>
        /// <returns><c>true</c> if [is conversion allowed] [the specified root type]; otherwise, <c>false</c>.</returns>
        private bool IsConversionAllowed(Type rootType, Type includedType)
        {
            string cacheKey = "Framework.AssetLibrary.Types.Mapping::IsCollection::" + rootType.FullName + "::" + includedType.FullName;

            ICacheContext cacheContext = CacheFactory.GetContext();            

            if (!cacheContext.Contains(cacheKey))
            {
                bool returnValue = false;

                CollectionDetails collectionDetails = includedType.IsCollection();

                if (collectionDetails.IsCollection)
                {
                    includedType = collectionDetails.CollectionElementType;
                }

                if (includedType.IsFrameworkType())
                    returnValue = true;

                if (!AllowOnlyIncludedTypes)
                    returnValue = true;

                MapHierarchy mapHierarchy;
                if (_mapsHierarchyDictionary.TryGetValue(rootType, out mapHierarchy))
                {
                    if (mapHierarchy.TypeChain.Contains(includedType))
                        returnValue = true;
                }

                cacheContext.Add<bool>(cacheKey, returnValue);

                return returnValue;
            }
            else
            {
                return cacheContext.Get<bool>(cacheKey);
            }           
        }

        /// <summary>
        /// Determines whether the specified included type is cyclical.
        /// </summary>
        /// <param name="includedType">Type of the included.</param>
        /// <param name="parentReference">The parent reference.</param>
        /// <returns><c>true</c> if the specified included type is cyclical; otherwise, <c>false</c>.</returns>
        private bool IsCyclical(Type includedType, out object parentReference)
        {
            //Initialize the output variables
            parentReference = null;

            if (_activeExecutionChain.Count > 0)
            {
                var peek = _activeExecutionChain.Peek();

                if (peek.Key == includedType)
                {
                    parentReference = peek.Value;
                    return true;
                }
            }

            return false;
        }

        #endregion Private methods
    }
}