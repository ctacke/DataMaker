using System.Linq.Expressions;

namespace DataMaker
{
    /// <summary>
    /// Configures property-to-data mappings for a specific entity type.
    /// </summary>
    /// <typeparam name="T">The entity type to map.</typeparam>
    public class DataMap<T> where T : new()
    {
        internal readonly string PrimaryTableName;
        internal readonly Dictionary<string, IPropertyMapping> Mappings = new Dictionary<string, IPropertyMapping>();
        private readonly Generator _generator;

        /// <summary>
        /// Initializes a new DataMap with the specified primary table.
        /// </summary>
        /// <param name="generator">The generator instance.</param>
        /// <param name="primaryTableName">The name of the primary table for this entity.</param>
        public DataMap(Generator generator, string primaryTableName)
        {
            if (string.IsNullOrWhiteSpace(primaryTableName))
            {
                throw new ArgumentException("Primary table name cannot be null or empty.", nameof(primaryTableName));
            }

            _generator = generator;
            PrimaryTableName = primaryTableName;
        }

        /// <summary>
        /// Maps a property to a column in the primary table.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.Name).</param>
        /// <param name="columnName">The name of the column in the primary table.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithColumn(u => u.Name, "FullName")
        /// </code>
        /// </example>
        public DataMap<T> WithColumn<TProp>(Expression<Func<T, TProp>> property, string columnName)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new ColumnMapping(columnName);
            return this;
        }

        /// <summary>
        /// Maps a property to a column in the primary table using the property name as the column name.
        /// Convenient when the property name matches the database column name.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.Name).</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// // Property Name matches column Name
        /// dataMap.WithColumn(u => u.Name)  // Maps to "Name" column
        /// dataMap.WithColumn(u => u.Email) // Maps to "Email" column
        /// </code>
        /// </example>
        public DataMap<T> WithColumn<TProp>(Expression<Func<T, TProp>> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new ColumnMapping(propertyName);
            return this;
        }

        /// <summary>
        /// Maps a property to a column in a related table using a foreign key lookup.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.City).</param>
        /// <param name="relatedTableName">The name of the related table to look up.</param>
        /// <param name="foreignKeyColumn">The foreign key column in the primary table.</param>
        /// <param name="relatedKeyColumn">The key column in the related table.</param>
        /// <param name="relatedColumnName">The column in the related table to retrieve.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithLookup(u => u.City, "Address", "AddressId", "Id", "City")
        /// </code>
        /// </example>
        public DataMap<T> WithLookup<TProp>(Expression<Func<T, TProp>> property, string relatedTableName, string foreignKeyColumn, string relatedKeyColumn, string relatedColumnName)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrWhiteSpace(relatedTableName))
            {
                throw new ArgumentException("Related table name cannot be null or empty.", nameof(relatedTableName));
            }

            if (string.IsNullOrWhiteSpace(foreignKeyColumn))
            {
                throw new ArgumentException("Foreign key column cannot be null or empty.", nameof(foreignKeyColumn));
            }

            if (string.IsNullOrWhiteSpace(relatedKeyColumn))
            {
                throw new ArgumentException("Related key column cannot be null or empty.", nameof(relatedKeyColumn));
            }

            if (string.IsNullOrWhiteSpace(relatedColumnName))
            {
                throw new ArgumentException("Related column name cannot be null or empty.", nameof(relatedColumnName));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new LookupMapping(relatedTableName, foreignKeyColumn, relatedKeyColumn, relatedColumnName);
            return this;
        }

        /// <summary>
        /// Maps a property to a data source using a custom mapping function that receives the primary row and data provider.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.FullName).</param>
        /// <param name="mappingFunc">Function that takes the primary row and data provider, and returns the value for this property.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithTableMap(u => u.FullName, (row, provider) => $"{row["FirstName"]} {row["LastName"]}")
        /// </code>
        /// </example>
        public DataMap<T> WithTableMap<TProp>(Expression<Func<T, TProp>> property, Func<IDataRow, IDataProvider, object> mappingFunc)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (mappingFunc == null)
            {
                throw new ArgumentNullException(nameof(mappingFunc));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new CustomMapping(mappingFunc);
            return this;
        }

        /// <summary>
        /// Maps a property to a data source using a custom mapping function that receives the primary row, data provider, and generation index.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.Id).</param>
        /// <param name="mappingFunc">Function that takes the primary row, data provider, and zero-based index, and returns the value for this property.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// // Generate sequential IDs starting at 1
        /// dataMap.WithTableMap(u => u.Id, (row, provider, index) => index + 1)
        ///
        /// // Generate custom sequential values
        /// dataMap.WithTableMap(u => u.OrderNumber, (row, provider, index) => $"ORD-{index + 1000:D5}")
        /// </code>
        /// </example>
        public DataMap<T> WithTableMap<TProp>(Expression<Func<T, TProp>> property, Func<IDataRow, IDataProvider, int, object> mappingFunc)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (mappingFunc == null)
            {
                throw new ArgumentNullException(nameof(mappingFunc));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new IndexedCustomMapping(mappingFunc);
            return this;
        }

        /// <summary>
        /// Maps a property to a sequential value starting from the specified start value.
        /// Useful for generating sequential IDs or numbers.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped (must be numeric).</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.Id).</param>
        /// <param name="startValue">The starting value for the sequence. Defaults to 1.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// // Generate IDs starting at 1: 1, 2, 3, 4...
        /// dataMap.WithSequence(u => u.Id)
        ///
        /// // Generate IDs starting at 1000: 1000, 1001, 1002...
        /// dataMap.WithSequence(u => u.OrderId, startValue: 1000)
        /// </code>
        /// </example>
        public DataMap<T> WithSequence<TProp>(Expression<Func<T, TProp>> property, int startValue = 1)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new SequenceMapping(startValue);
            return this;
        }

        /// <summary>
        /// Maps a property to a generated value using a function.
        /// Useful for generating GUIDs, timestamps, or other computed values.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., p => p.Id).</param>
        /// <param name="valueFunc">Function that generates the value for this property.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// // Generate GUIDs
        /// dataMap.WithValue(p => p.Id, () => Guid.NewGuid())
        ///
        /// // Generate timestamps
        /// dataMap.WithValue(p => p.CreatedAt, () => DateTime.UtcNow)
        ///
        /// // Generate random values
        /// dataMap.WithValue(p => p.RandomNumber, () => Random.Shared.Next(1, 100))
        /// </code>
        /// </example>
        public DataMap<T> WithValue<TProp>(Expression<Func<T, TProp>> property, Func<object> valueFunc)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (valueFunc == null)
            {
                throw new ArgumentNullException(nameof(valueFunc));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new ValueMapping(valueFunc);
            return this;
        }

        /// <summary>
        /// Maps a property to a generated value using a function that receives the generation index.
        /// Useful for generating indexed values, formatted IDs, or sequential computed values.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., p => p.Code).</param>
        /// <param name="valueFunc">Function that takes the zero-based generation index and returns the value for this property.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// // Generate formatted product codes
        /// dataMap.WithValue(p => p.Code, index => $"PROD-{index + 1:D5}")  // PROD-00001, PROD-00002...
        ///
        /// // Generate email addresses with index
        /// dataMap.WithValue(p => p.Email, index => $"user{index + 1}@example.com")
        ///
        /// // Generate alternating values based on index
        /// dataMap.WithValue(p => p.Type, index => index % 2 == 0 ? "TypeA" : "TypeB")
        /// </code>
        /// </example>
        public DataMap<T> WithValue<TProp>(Expression<Func<T, TProp>> property, Func<int, object> valueFunc)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (valueFunc == null)
            {
                throw new ArgumentNullException(nameof(valueFunc));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new IndexedValueMapping(valueFunc);
            return this;
        }

        /// <summary>
        /// Maps a property to a transformed column in the primary table.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.ShortName).</param>
        /// <param name="columnName">The name of the column in the primary table to transform.</param>
        /// <param name="transformFunc">Function that takes the generation index and original column value, and returns the transformed value.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithColumnTransform(p => p.ShortName, "LongName", (index, value) => value.ToString().Substring(0, 10))
        /// </code>
        /// </example>
        public DataMap<T> WithColumnTransform<TProp>(Expression<Func<T, TProp>> property, string columnName, Func<int, object?, object?> transformFunc)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));
            }

            if (transformFunc == null)
            {
                throw new ArgumentNullException(nameof(transformFunc));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new ColumnTransformMapping(columnName, transformFunc);
            return this;
        }

        /// <summary>
        /// Maps a property to a generated person name using a custom formatter.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.Name).</param>
        /// <param name="formatter">Function that takes a <see cref="PersonData"/> object and returns the formatted value.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithPersonData(u => u.Name, (data) => $"{data.FirstName} {data.LastName}")
        /// dataMap.WithPersonData(u => u.Name, (data) => $"{data.LastName}, {data.FirstName}")
        /// </code>
        /// </example>
        public DataMap<T> WithPersonData<TProp>(Expression<Func<T, TProp>> property, Func<PersonData, object> formatter)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new PersonDataMapping(formatter);
            return this;
        }

        /// <summary>
        /// Maps a property to a generated address value using a custom accessor.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.City).</param>
        /// <param name="accessor">Function that takes an <see cref="AddressData"/> object and returns the desired field.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithAddressData(u => u.Street, a => a.StreetAddress)
        /// dataMap.WithAddressData(u => u.City, a => a.City)
        /// dataMap.WithAddressData(u => u.State, a => a.State)
        /// </code>
        /// </example>
        public DataMap<T> WithAddressData<TProp>(Expression<Func<T, TProp>> property, Func<AddressData, object> accessor)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (accessor == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new AddressDataMapping(_generator, accessor);
            return this;
        }

        /// <summary>
        /// Maps a property to a deterministic long ID (Snowflake-like).
        /// Same seed produces the same sequence of IDs across test runs.
        /// </summary>
        /// <param name="property">Expression identifying the property to map.</param>
        /// <param name="seed">Seed value for deterministic generation.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithDeterministicLong(c => c.Id, seed: 12345)
        /// </code>
        /// </example>
        public DataMap<T> WithDeterministicLong(Expression<Func<T, long>> property, int seed)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new DeterministicLongMapping(seed);
            return this;
        }

        /// <summary>
        /// Maps a property to a deterministic int ID.
        /// Same seed produces the same sequence of IDs across test runs.
        /// </summary>
        /// <param name="property">Expression identifying the property to map.</param>
        /// <param name="seed">Seed value for deterministic generation.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        public DataMap<T> WithDeterministicInt(Expression<Func<T, int>> property, int seed)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new DeterministicIntMapping(seed);
            return this;
        }

        /// <summary>
        /// Maps a property to a deterministic GUID.
        /// Same seed produces the same sequence of GUIDs across test runs.
        /// </summary>
        /// <param name="property">Expression identifying the property to map.</param>
        /// <param name="seed">Seed value for deterministic generation.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        public DataMap<T> WithDeterministicGuid(Expression<Func<T, Guid>> property, int seed)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new DeterministicGuidMapping(seed);
            return this;
        }

        /// <summary>
        /// Maps a property to a deterministic string ID.
        /// Same seed produces the same sequence of strings across test runs.
        /// </summary>
        /// <param name="property">Expression identifying the property to map.</param>
        /// <param name="seed">Seed value for deterministic generation.</param>
        /// <param name="length">Length of the random portion (default 8).</param>
        /// <param name="prefix">Optional prefix for the ID.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithDeterministicString(c => c.Code, seed: 12345, length: 10, prefix: "USR-")
        /// </code>
        /// </example>
        public DataMap<T> WithDeterministicString(Expression<Func<T, string>> property, int seed, int length = 8, string? prefix = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new DeterministicStringMapping(seed, length, prefix);
            return this;
        }

        private string GetPropertyName<TProp>(Expression<Func<T, TProp>> property)
        {
            if (property.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
            throw new ArgumentException("Expression is not a member access", nameof(property));
        }
    }

    /// <summary>
    /// Represents a property mapping strategy.
    /// </summary>
    internal interface IPropertyMapping
    {
        object? GetValue(IDataRow primaryRow, IDataProvider provider, int index);
    }

    /// <summary>
    /// Maps a property to a column in the primary table.
    /// </summary>
    internal class ColumnMapping : IPropertyMapping
    {
        private readonly string _columnName;

        public ColumnMapping(string columnName)
        {
            _columnName = columnName;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return primaryRow[_columnName];
        }
    }

    /// <summary>
    /// Maps a property using a foreign key lookup to a related table.
    /// </summary>
    internal class LookupMapping : IPropertyMapping
    {
        private readonly string _relatedTableName;
        private readonly string _foreignKeyColumn;
        private readonly string _relatedKeyColumn;
        private readonly string _relatedColumnName;

        public LookupMapping(string relatedTableName, string foreignKeyColumn, string relatedKeyColumn, string relatedColumnName)
        {
            _relatedTableName = relatedTableName;
            _foreignKeyColumn = foreignKeyColumn;
            _relatedKeyColumn = relatedKeyColumn;
            _relatedColumnName = relatedColumnName;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            var foreignKeyValue = primaryRow[_foreignKeyColumn];
            if (foreignKeyValue == null || foreignKeyValue == DBNull.Value)
            {
                return null;
            }

            var relatedTable = provider[_relatedTableName];
            var relatedRow = relatedTable.FirstOrDefault(row =>
            {
                var keyValue = row[_relatedKeyColumn];
                return keyValue != null && keyValue.Equals(foreignKeyValue);
            });

            return relatedRow?[_relatedColumnName];
        }
    }

    /// <summary>
    /// Maps a property using a custom function.
    /// </summary>
    internal class CustomMapping : IPropertyMapping
    {
        private readonly Func<IDataRow, IDataProvider, object> _mappingFunc;

        public CustomMapping(Func<IDataRow, IDataProvider, object> mappingFunc)
        {
            _mappingFunc = mappingFunc;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _mappingFunc(primaryRow, provider);
        }
    }

    /// <summary>
    /// Maps a property using a custom function that has access to the generation index.
    /// </summary>
    internal class IndexedCustomMapping : IPropertyMapping
    {
        private readonly Func<IDataRow, IDataProvider, int, object> _mappingFunc;

        public IndexedCustomMapping(Func<IDataRow, IDataProvider, int, object> mappingFunc)
        {
            _mappingFunc = mappingFunc;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _mappingFunc(primaryRow, provider, index);
        }
    }

    /// <summary>
    /// Maps a property to a sequential numeric value.
    /// </summary>
    internal class SequenceMapping : IPropertyMapping
    {
        private readonly int _startValue;

        public SequenceMapping(int startValue)
        {
            _startValue = startValue;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _startValue + index;
        }
    }

    /// <summary>
    /// Maps a property to a generated value.
    /// </summary>
    internal class ValueMapping : IPropertyMapping
    {
        private readonly Func<object> _valueFunc;

        public ValueMapping(Func<object> valueFunc)
        {
            _valueFunc = valueFunc;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _valueFunc();
        }
    }

    /// <summary>
    /// Maps a property to a generated value with access to the generation index.
    /// </summary>
    internal class IndexedValueMapping : IPropertyMapping
    {
        private readonly Func<int, object> _valueFunc;

        public IndexedValueMapping(Func<int, object> valueFunc)
        {
            _valueFunc = valueFunc;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _valueFunc(index);
        }
    }

    /// <summary>
    /// Maps a property to a transformed column in the primary table.
    /// </summary>
    internal class ColumnTransformMapping : IPropertyMapping
    {
        private readonly string _columnName;
        private readonly Func<int, object?, object?> _transformFunc;

        public ColumnTransformMapping(string columnName, Func<int, object?, object?> transformFunc)
        {
            _columnName = columnName;
            _transformFunc = transformFunc;
        }

        public object? GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            var originalValue = primaryRow[_columnName];
            return _transformFunc(index, originalValue);
        }
    }

    /// <summary>
    /// Maps a property to a deterministic long ID using IdGenerator.
    /// </summary>
    internal class DeterministicLongMapping : IPropertyMapping
    {
        private readonly IdGenerator _idGenerator;

        public DeterministicLongMapping(int seed)
        {
            _idGenerator = new IdGenerator(seed);
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _idGenerator.NextLong();
        }
    }

    /// <summary>
    /// Maps a property to a deterministic int ID using IdGenerator.
    /// </summary>
    internal class DeterministicIntMapping : IPropertyMapping
    {
        private readonly IdGenerator _idGenerator;

        public DeterministicIntMapping(int seed)
        {
            _idGenerator = new IdGenerator(seed);
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _idGenerator.NextInt();
        }
    }

    /// <summary>
    /// Maps a property to a deterministic GUID using IdGenerator.
    /// </summary>
    internal class DeterministicGuidMapping : IPropertyMapping
    {
        private readonly IdGenerator _idGenerator;

        public DeterministicGuidMapping(int seed)
        {
            _idGenerator = new IdGenerator(seed);
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _idGenerator.NextGuid();
        }
    }

    /// <summary>
    /// Maps a property to a deterministic string ID using IdGenerator.
    /// </summary>
    internal class DeterministicStringMapping : IPropertyMapping
    {
        private readonly IdGenerator _idGenerator;
        private readonly int _length;
        private readonly string? _prefix;

        public DeterministicStringMapping(int seed, int length, string? prefix)
        {
            _idGenerator = new IdGenerator(seed);
            _length = length;
            _prefix = prefix;
        }

        public object GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            return _idGenerator.NextString(_length, _prefix);
        }
    }
}
