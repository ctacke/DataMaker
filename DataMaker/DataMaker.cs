using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMaker
{
    /// <summary>
    /// Main class for generating test data entities from data sources.
    /// </summary>
    public class Generator
    {
        private IDataProvider? _dataProvider;
        private readonly Dictionary<Type, object> _dataMaps = new Dictionary<Type, object>();
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the Generator class.
        /// </summary>
        public Generator()
        {
        }

        /// <summary>
        /// Adds a data provider to be used for generating test data.
        /// </summary>
        /// <param name="provider">The data provider implementation.</param>
        public void AddProvider(IDataProvider provider)
        {
            _dataProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Creates and registers a data map for the specified type with a primary table.
        /// </summary>
        /// <typeparam name="T">The type to create a data map for.</typeparam>
        /// <param name="primaryTableName">The name of the primary table for this entity.</param>
        /// <returns>A DataMap instance for configuring property mappings.</returns>
        public DataMap<T> AddDataMap<T>(string primaryTableName) where T : new()
        {
            var map = new DataMap<T>(primaryTableName);
            _dataMaps[typeof(T)] = map;
            return map;
        }

        /// <summary>
        /// Generates a collection of test data entities using the configured data mappings.
        /// </summary>
        /// <typeparam name="T">The type of entity to generate.</typeparam>
        /// <param name="count">The number of entities to generate.</param>
        /// <param name="strategy">The selection strategy (Sequential or Random). Defaults to Sequential.</param>
        /// <returns>A collection of generated entities.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no data provider has been added or no data map exists for the type.</exception>
        public IEnumerable<T> Generate<T>(int count, SelectionStrategy strategy = SelectionStrategy.Sequential) where T : new()
        {
            if (_dataProvider == null)
            {
                throw new InvalidOperationException("No data provider has been added. Call AddProvider() before generating data.");
            }

            if (!_dataMaps.TryGetValue(typeof(T), out var dataMap))
            {
                throw new InvalidOperationException($"No data map found for type {typeof(T).Name}. Call AddDataMap<{typeof(T).Name}>() to configure mappings.");
            }

            var map = (DataMap<T>)dataMap;
            var primaryTable = _dataProvider[map.PrimaryTableName];
            var rowCount = primaryTable.GetRowCount();

            if (rowCount == 0)
            {
                throw new InvalidOperationException($"Primary table '{map.PrimaryTableName}' has no rows.");
            }

            var list = new List<T>();

            for (var i = 0; i < count; i++)
            {
                // Determine which row to use from the primary table based on strategy
                int rowIndex;
                if (strategy == SelectionStrategy.Sequential)
                {
                    rowIndex = i % rowCount; // Wrap around if count > rowCount
                }
                else // Random
                {
                    rowIndex = _random.Next(0, rowCount);
                }

                // Get the primary row
                var primaryRow = primaryTable[rowIndex];

                // Create the entity and populate its properties
                var item = new T();
                foreach (var mapping in map.Mappings)
                {
                    var property = typeof(T).GetProperty(mapping.Key);
                    if (property != null)
                    {
                        // Use the mapping strategy to get the value, passing the generation index
                        var value = mapping.Value.GetValue(primaryRow, _dataProvider, i);

                        // Set the property value with type conversion
                        if (value != null && value != DBNull.Value)
                        {
                            if (property.PropertyType != value.GetType())
                            {
                                try
                                {
                                    property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                                }
                                catch (InvalidCastException)
                                {
                                    // If conversion fails, try setting directly (might be nullable)
                                    property.SetValue(item, value);
                                }
                            }
                            else
                            {
                                property.SetValue(item, value);
                            }
                        }
                        else
                        {
                            // Set to null for nullable types or default for value types
                            property.SetValue(item, null);
                        }
                    }
                }
                list.Add(item);
            }

            return list;
        }
    }
}
