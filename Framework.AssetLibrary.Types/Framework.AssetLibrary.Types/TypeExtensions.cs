using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Framework.AssetLibrary.Caching;
using System.Data;
using System.ComponentModel;
using Framework.AssetLibrary.Types.Mapping;

namespace Framework.AssetLibrary.Types
{
    public static class TypeExtensions
    {
        #region Private variables

        private static readonly List<byte[]> tokens = new List<byte[]>
        {
            new byte[] {0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89},
            new byte[] {0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35},
            new byte[] {0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a}
        };

        #endregion Private variables

        #region Private classes / Data structures

        /// <summary>
        ///     Class ByteArrayEqualityComparer.
        /// </summary>
        private class ByteArrayEqualityComparer : EqualityComparer<byte[]>
        {
            public override bool Equals(byte[] x, byte[] y)
            {
                return x != null && y != null && x.Length == 8 && y.Length == 8 && x[0] == y[0] && x[1] == y[1] &&
                       x[2] == y[2] && x[3] == y[3] && x[4] == y[4] && x[5] == y[5] && x[6] == y[6] && x[7] == y[7];
            }

            public override int GetHashCode(byte[] obj)
            {
                return obj.GetHashCode();
            }
        }

        #endregion Private classes / Data structures

        #region Public methods

        /// <summary>
        /// Determines whether the specified type is collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>CollectionDetails.</returns>
        public static CollectionDetails IsCollection(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string cacheKey = "Framework.AssetLibrary.Types::IsCollection::" + type.FullName;

            ICacheContext cacheContext = CacheFactory.GetContext();

            if (!cacheContext.Contains(cacheKey))
            {
                CollectionDetails collectionDetails = new CollectionDetails();

                #region Arrays

                if (type.IsArray)
                {
                    collectionDetails.IsCollection = true;
                    collectionDetails.CollectionElementType = type.GetElementType(); ;
                    collectionDetails.CollectionType = CollectionType.Array;
                    return collectionDetails;
                }

                #endregion Arrays

                #region Generic collections

                if (type.GetInterface(typeof(ICollection<>).FullName) != null)
                {
                    collectionDetails.IsCollection = true;
                    collectionDetails.CollectionElementType = type.GetGenericArguments()[0];
                    collectionDetails.CollectionType = CollectionType.ICollectionT;
                    return collectionDetails;
                }

                if (type.GetInterface(typeof(IList<>).FullName) != null)
                {
                    collectionDetails.IsCollection = true;
                    collectionDetails.CollectionElementType = type.GetGenericArguments()[0];
                    collectionDetails.CollectionType = CollectionType.IListT;
                    return collectionDetails;
                }

                if (type.GetInterface(typeof(IEnumerable<>).FullName) != null)
                {
                    //Only if its not a string since strings are recognized as char enumerable
                    if ((type.FullName) != typeof(string).FullName)
                    {
                        collectionDetails.IsCollection = true;
                        collectionDetails.CollectionElementType = type.GetGenericArguments()[0];
                        collectionDetails.CollectionType = CollectionType.IEnumerableT;
                        return collectionDetails;
                    }
                }

                if (type.GetInterface(typeof(IEnumerable).FullName) != null)
                {
                    //Only if its not a string since strings are recognized as char enumerable
                    if ((type.FullName) != typeof(string).FullName)
                    {
                        collectionDetails.IsCollection = true;
                        collectionDetails.CollectionElementType = type.GetGenericArguments()[0];
                        collectionDetails.CollectionType = CollectionType.IEnumerableT;
                        return collectionDetails;
                    }
                }

                #endregion Generic collections

                #region Non generic collections

                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    //Only if its not a string since strings are recognized as char enumerable
                    if ((type.FullName) != typeof(string).FullName)
                    {
                        collectionDetails.IsCollection = true;
                        collectionDetails.CollectionElementType = type;
                        collectionDetails.CollectionType = CollectionType.IEnumerable;
                        return collectionDetails;
                    }
                }

                if (typeof(ICollection).IsAssignableFrom(type))
                {
                    //Only if its not a string since strings are recognized as char enumerable
                    if ((type.FullName) != typeof(string).FullName)
                    {
                        collectionDetails.IsCollection = true;
                        collectionDetails.CollectionElementType = type;
                        collectionDetails.CollectionType = CollectionType.ICollection;
                        return collectionDetails;
                    }
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    //Only if its not a string since strings are recognized as char enumerable
                    if ((type.FullName) != typeof(string).FullName)
                    {
                        collectionDetails.IsCollection = true;
                        collectionDetails.CollectionElementType = type;
                        collectionDetails.CollectionType = CollectionType.IList;
                        return collectionDetails;
                    }
                }

                #endregion Non generic collections

                collectionDetails.IsCollection = false;
                collectionDetails.CollectionElementType = null;
                collectionDetails.CollectionType = CollectionType.NotDefined;

                cacheContext.Add<CollectionDetails>(cacheKey, collectionDetails);

                return collectionDetails;
            }
            else
            {
                return cacheContext.Get<CollectionDetails>(cacheKey);
            }
            
        }

        /// <summary>
        ///     Checks if its a native framework type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsFrameworkType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string cacheKey = "Framework.AssetLibrary.Types::IsFrameworkType::" + type.FullName;

            ICacheContext cacheContext = CacheFactory.GetContext();

            if (!cacheContext.Contains(cacheKey))
            {
                var publicKeyToken = type.Assembly.GetName().GetPublicKeyToken();

                if (publicKeyToken != null && publicKeyToken.Length == 8 && tokens.Contains(publicKeyToken, new ByteArrayEqualityComparer()))
                {
                    cacheContext.Add<bool>(cacheKey, true);
                    return true;
                }
                else
                {
                    cacheContext.Add<bool>(cacheKey, false);
                    return false;
                }
            }
            else
            {
                return cacheContext.Get<bool>(cacheKey);
            }
        }

        /// <summary>
        /// Determines whether [is value type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is value type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsStringOrValueType(this Type type)
        {
            if (type.IsValueType || type.Equals(typeof(string)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// To the data table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>DataTable.</returns>
        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            DataTable table = new DataTable();

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            if (typeof(T).IsStringOrValueType())
            {
                string genericColumnName = "Value";

                table.Columns.Add(genericColumnName, typeof(T));

                foreach (T item in list)
                {
                    DataRow row = table.NewRow();
                    row[genericColumnName] = item;
                    table.Rows.Add(row);
                }
            }
            else
            {
                foreach (PropertyDescriptor prop in properties)
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }

                foreach (T item in list)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        try
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        catch
                        {
                            row[prop.Name] = DBNull.Value;
                        }
                    }

                    table.Rows.Add(row);
                }
            }    

            return table;
        }

        /// <summary>
        /// To the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <returns>List&lt;T&gt;.</returns>
        public static List<T> ToList<T>(this DataTable table)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            List<T> returnList = new List<T>();

            if (typeof(T).IsStringOrValueType())
            {
                foreach (DataRow aRow in table.Rows)
                {  
                    returnList.Add((T)aRow[0]);
                }
            }
            else
            {
                foreach (DataRow aRow in table.Rows)
                {
                    T aObject = Activator.CreateInstance<T>();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        if (aRow[prop.Name] != null)
                        {
                            prop.SetValue(aObject, aRow[prop.Name]);
                        }
                    }
                    returnList.Add(aObject);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Converts the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>D.</returns>
        public static D Convert<T, D>(this T data) where T : class where D : class
        {
            var mapper = new MapperFactory().GetContext();
            mapper.AddMap<T, D>();
            return mapper.Map<T, D>(data);
        }

        #endregion Public methods
    }
}