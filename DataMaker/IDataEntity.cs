namespace DataMaker;

/// <summary>
/// Represents a collection of data rows (e.g., a database table).
/// </summary>
public interface IDataEntity : IEnumerable<IDataRow>
{
    /// <summary>
    /// Gets the data row at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the row to retrieve.</param>
    /// <returns>The data row at the specified index.</returns>
    IDataRow this[int index] { get; }

    /// <summary>
    /// Gets the total number of rows in this data entity.
    /// </summary>
    int RowCount { get; }

    /// <summary>
    /// Returns the total number of rows in this data entity.
    /// </summary>
    /// <returns>The row count.</returns>
    int GetRowCount();
}