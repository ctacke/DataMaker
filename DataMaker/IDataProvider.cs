namespace DataMaker;

/// <summary>
/// Provides access to data entities from a data source.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Gets the data entity with the specified name.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., table name).</param>
    /// <returns>An IDataEntity representing the data collection.</returns>
    IDataEntity this[string entityName] { get; }
}