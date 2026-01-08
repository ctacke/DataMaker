using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DataMaker
{
    /// <summary>
    /// Data provider implementation for SQL Server databases.
    /// </summary>
    public class SqlServerDataProvider : IDataProvider
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, IDataEntity> _tables = new Dictionary<string, IDataEntity>(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex TableNameValidationRegex = new Regex(@"^[\w\.\[\]]+$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the SqlServerDataProvider class.
        /// </summary>
        /// <param name="connectionString">The SQL Server connection string.</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string is null.</exception>
        public SqlServerDataProvider(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($"SELECT * FROM {tableName}", connection);
                var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return new DataTableWrapper(dataTable);
            }
        }
    }

    internal class DataTableWrapper : IDataEntity
    {
        private readonly DataTable _dataTable;

        public DataTableWrapper(DataTable dataTable)
        {
            _dataTable = dataTable;
        }

        public IDataRow this[int index] => new DataRowWrapper(_dataTable.Rows[index]);

        public int RowCount => _dataTable.Rows.Count;
        public int GetRowCount() => _dataTable.Rows.Count;

        public IEnumerator<IDataRow> GetEnumerator()
        {
            foreach (DataRow row in _dataTable.Rows)
            {
                yield return new DataRowWrapper(row);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class DataRowWrapper : IDataRow
    {
        private readonly DataRow _dataRow;

        public DataRowWrapper(DataRow dataRow)
        {
            _dataRow = dataRow;
        }

        public object this[string columnName] => _dataRow[columnName];
    }
}