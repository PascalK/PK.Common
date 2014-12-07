using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace PK.Common
{
    /// <summary>
    /// Base class for an enumerated type, this implements a generic descriptor pattern
    /// </summary>
    /// <typeparam name="TEnumerated">The type that is enumerated</typeparam>
    /// <typeparam name="TValue">The value of the enumerated type</typeparam>
    [DataContract]
    public abstract class Enumerated<TEnumerated, TValue> : IEnumerated<TEnumerated, TValue>
        where TEnumerated : class, IEnumerated<TEnumerated, TValue>
    {
        private static DictionaryWithNullKey<TValue, TEnumerated> _dict = null;


        /// <summary>
        /// The value of the enumerated type
        /// </summary>
        /// <remarks>
        /// Usually this will be an identifier of the enumerated type
        /// </remarks>
        [DataMember]
        public virtual TValue Value { get; private set; }

        static Enumerated()
        {
            //force type init for the descriptor
            RuntimeHelpers.RunClassConstructor(typeof(TEnumerated).TypeHandle);
        }
        /// <summary>
        /// Initializes a new instance of the Enumerated class with a value indicated by a TValue
        /// </summary>
        /// <param name="value">The value if the enumerated instance to create</param>
        public Enumerated(TValue value)
        {
            InitDictionary();
            Value = value;

            TEnumerated enumValue;

            enumValue = this as TEnumerated;
            if (value != null)
            {
                _dict.Add(value, enumValue);
            }
            else
            {
                _dict.NullValue = enumValue;
            }
        }

        private void InitDictionary()
        {
            if (_dict == null)
            {
                lock (TypeLock<TEnumerated>.SyncLock)
                {
                    if (_dict == null)
                    {
                        _dict = new DictionaryWithNullKey<TValue, TEnumerated>();
                    }
                }
            }
        }

        /// <summary>
        /// Get all defined enumerated items
        /// </summary>
        /// <returns>All defined enumerated items</returns>
        public static IEnumerable<TEnumerated> GetAll()
        {
            return _dict.Values;
        }
        /// <summary>
        /// Gets the enumerated item with the specified value
        /// </summary>
        /// <param name="value">The value of the enumerated to get</param>
        /// <returns>The enumerated item with the specified value</returns>
        /// <exception cref="InvalidOperationException">If no enumerated item with the specified value exists</exception>
        public static TEnumerated Get(TValue value)
        {
            TEnumerated foundValue;

            foundValue = GetOrDefault(value);
            if (foundValue != null)
            {
                return foundValue;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("No '{0}' found with value '{1}'",
                        typeof(TEnumerated).Name,
                        value != null ? value.ToString() : "NULL"));
            }
        }
        /// <summary>
        /// Gets the only enumerated item with the specified value or a default value if none exists
        /// </summary>
        /// <param name="value">The value of the enumerated to get</param>
        /// <returns>The enumerated item with the specified value or null if none exists</returns>
        public static TEnumerated GetOrDefault(TValue value)
        {
            TEnumerated foundValue;
            if (value != null)
            {
                if (_dict.ContainsKey(value))
                {
                    foundValue = _dict[value];
                }
                else
                {
                    foundValue = null;
                }
            }
            else
            {
                if (_dict.NullValue != null)
                {
                    foundValue = _dict.NullValue;
                }
                else
                {
                    foundValue = null;
                }
            }

            return foundValue;
        }

        private static class TypeLock<T>
        {
            public static readonly object SyncLock = new object();
        }
        private class DictionaryWithNullKey<TKey, TDictValue> : Dictionary<TKey, TDictValue>
        {
            public TDictValue NullValue { get; set; }
        }
    }
}
