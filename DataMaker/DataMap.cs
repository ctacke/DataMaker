using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Initializes a new DataMap with the specified primary table.
        /// </summary>
        /// <param name="primaryTableName">The name of the primary table for this entity.</param>
        public DataMap(string primaryTableName)
        {
            if (string.IsNullOrWhiteSpace(primaryTableName))
                throw new ArgumentException("Primary table name cannot be null or empty.", nameof(primaryTableName));

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
        /// dataMap.WithColumn(u => u.Name, "Name")
        /// </code>
        /// </example>
        public DataMap<T> WithColumn<TProp>(Expression<Func<T, TProp>> property, string columnName)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new ColumnMapping(columnName);
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
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (string.IsNullOrWhiteSpace(relatedTableName)) throw new ArgumentException("Related table name cannot be null or empty.", nameof(relatedTableName));
            if (string.IsNullOrWhiteSpace(foreignKeyColumn)) throw new ArgumentException("Foreign key column cannot be null or empty.", nameof(foreignKeyColumn));
            if (string.IsNullOrWhiteSpace(relatedKeyColumn)) throw new ArgumentException("Related key column cannot be null or empty.", nameof(relatedKeyColumn));
            if (string.IsNullOrWhiteSpace(relatedColumnName)) throw new ArgumentException("Related column name cannot be null or empty.", nameof(relatedColumnName));

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
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (mappingFunc == null) throw new ArgumentNullException(nameof(mappingFunc));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = new CustomMapping(mappingFunc);
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
        object GetValue(IDataRow primaryRow, IDataProvider provider);
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

        public object GetValue(IDataRow primaryRow, IDataProvider provider)
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

        public object GetValue(IDataRow primaryRow, IDataProvider provider)
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

        public object GetValue(IDataRow primaryRow, IDataProvider provider)
        {
            return _mappingFunc(primaryRow, provider);
        }
    }
}
