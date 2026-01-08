namespace DataMaker
{
    /// <summary>
    /// Represents a single row of data with column-based access.
    /// </summary>
    public interface IDataRow
    {
        /// <summary>
        /// Gets the value of the column with the specified name.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The value in the specified column.</returns>
        object this[string columnName] { get; }
    }
}
