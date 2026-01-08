using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataMaker
{
    /// <summary>
    /// Configures property-to-data mappings for a specific entity type.
    /// </summary>
    /// <typeparam name="T">The entity type to map.</typeparam>
    public class DataMap<T> where T : new()
    {
        internal readonly Dictionary<string, (Func<IDataRow, object> MappingFunc, string TableName)> Mappings = new Dictionary<string, (Func<IDataRow, object> MappingFunc, string TableName)>();

        /// <summary>
        /// Maps a property to a data source using a custom mapping function.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
        /// <param name="property">Expression identifying the property to map (e.g., u => u.Email).</param>
        /// <param name="tableName">The name of the table to retrieve data from.</param>
        /// <param name="mappingFunc">Function that takes a data row and returns the value for this property.</param>
        /// <returns>The DataMap instance for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// dataMap.WithTableMap(u => u.Email, "Users", row => row["Email"])
        /// dataMap.WithTableMap(u => u.FullName, "Users", row => $"{row["FirstName"]} {row["LastName"]}")
        /// </code>
        /// </example>
        public DataMap<T> WithTableMap<TProp>(Expression<Func<T, TProp>> property, string tableName, Func<IDataRow, object> mappingFunc)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
            if (mappingFunc == null) throw new ArgumentNullException(nameof(mappingFunc));

            var propertyName = GetPropertyName(property);
            Mappings[propertyName] = (mappingFunc, tableName);
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
}
