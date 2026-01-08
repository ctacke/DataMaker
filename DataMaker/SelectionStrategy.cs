namespace DataMaker
{
    /// <summary>
    /// Defines the strategy for selecting rows from data sources.
    /// </summary>
    public enum SelectionStrategy
    {
        /// <summary>
        /// Selects rows sequentially in order, wrapping around if more items are requested than available.
        /// </summary>
        Sequential,

        /// <summary>
        /// Selects rows randomly from the available data.
        /// </summary>
        Random
    }
}
