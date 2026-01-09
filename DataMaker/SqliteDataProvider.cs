using Microsoft.Data.Sqlite;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;

namespace DataMaker
{
    /// <summary>
    /// Data provider implementation for SQLite databases.
    /// </summary>
    public class SqliteDataProvider : IDataProvider
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, IDataEntity> _tables = new Dictionary<string, IDataEntity>(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex TableNameValidationRegex = new Regex(@"^[\w\.\[\]]+$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the SqliteDataProvider class.
        /// </summary>
        /// <param name="connectionString">The SQLite connection string or database file path.</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string is null.</exception>
        public SqliteDataProvider(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            if (!File.Exists(connectionString) && !connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                throw new FileNotFoundException($"The SQLite database file '{connectionString}' was not found.");
            }

            // If the connection string doesn't contain "Data Source=", treat it as a file path
            if (!_connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                _connectionString = $"Data Source={_connectionString}";
            }
        }

        /// <summary>
        /// Gets the data entity (table) with the specified name.
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve.</param>
        /// <returns>An IDataEntity representing the table data.</returns>
        /// <exception cref="ArgumentException">Thrown when table name is invalid or contains potentially malicious characters.</exception>
        public IDataEntity this[string tableName]
        {
            get
            {
                if (!_tables.ContainsKey(tableName))
                {
                    _tables[tableName] = GetTable(tableName);
                }
                return _tables[tableName];
            }
        }

        private IDataEntity GetTable(string tableName)
        {
            // Validate table name to prevent SQL injection
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
            }

            if (!TableNameValidationRegex.IsMatch(tableName))
            {
                throw new ArgumentException($"Invalid table name '{tableName}'. Table names must contain only letters, numbers, underscores, periods, and brackets.", nameof(tableName));
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = new SqliteCommand($"SELECT * FROM {tableName}", connection);

                var dataTable = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dataTable.Load(reader);
                }

                return new SqliteDataTableWrapper(dataTable);
            }
        }
    }

    /// <summary>
    /// Wraps a DataTable for SQLite data access.
    /// </summary>
    internal class SqliteDataTableWrapper : IDataEntity
    {
        private readonly DataTable _dataTable;

        public SqliteDataTableWrapper(DataTable dataTable)
        {
            _dataTable = dataTable;
        }

        public IDataRow this[int index] => new SqliteDataRowWrapper(_dataTable.Rows[index]);

        public int RowCount => _dataTable.Rows.Count;
        public int GetRowCount() => _dataTable.Rows.Count;

        public IEnumerator<IDataRow> GetEnumerator()
        {
            foreach (DataRow row in _dataTable.Rows)
            {
                yield return new SqliteDataRowWrapper(row);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Wraps a DataRow for SQLite data access.
    /// </summary>
    internal class SqliteDataRowWrapper : IDataRow
    {
        private readonly DataRow _dataRow;

        public SqliteDataRowWrapper(DataRow dataRow)
        {
            _dataRow = dataRow;
        }

        public object this[string columnName] => _dataRow[columnName];
    }
}
