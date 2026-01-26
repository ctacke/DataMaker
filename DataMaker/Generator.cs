namespace DataMaker;

/// <summary>
/// Main class for generating test data entities from data sources.
/// </summary>
public class Generator
{
    private IDataProvider? _dataProvider;
    private readonly Dictionary<Type, object> _dataMaps = new Dictionary<Type, object>();
    private readonly Random _random = new Random();
    private readonly Dictionary<int, AddressData> _addressCache = new();

    /// <summary>
    /// Initializes a new instance of the Generator class.
    /// </summary>
    public Generator()
    {
    }

    internal AddressData GetAddress(int index)
    {
        if (!_addressCache.TryGetValue(index, out var addressData))
        {
            addressData = AddressDataGenerator.Generate();
            _addressCache[index] = addressData;
        }
        return addressData;
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
        var map = new DataMap<T>(this, primaryTableName);
        _dataMaps[typeof(T)] = map;
        return map;
    }

    /// <summary>
    /// Generates a collection of test data entities using the configured data mappings.
    /// </summary>
    /// <typeparam name="T">The type of entity to generate.</typeparam>
    /// <param name="count">The number of entities to generate.</param>
    /// <param name="strategy">The selection strategy (Sequential or Random). Defaults to Sequential.</param>
    /// <param name="allowRepeats">If true, the generator may select the same row multiple times. If false, then it may return fewer than the requested number of rows.</param>
    /// <returns>A GeneratedQuery that can be enumerated or filtered with FirstAdd().</returns>
    /// <exception cref="InvalidOperationException">Thrown when no data provider has been added or no data map exists for the type.</exception>
    public GeneratedQuery<T> Generate<T>(int count, SelectionStrategy strategy = SelectionStrategy.Sequential, bool allowRepeats = false) where T : new()
    {
        return new GeneratedQuery<T>(this, count, strategy, allowRepeats);
    }

    /// <summary>
    /// Internal method that performs the actual generation with predicate support.
    /// When predicates are provided via FirstAdd(), matching items are yielded first,
    /// then remaining slots are filled with other items.
    /// </summary>
    internal IEnumerable<T> GenerateInternal<T>(int count, SelectionStrategy strategy, bool allowRepeats, List<Func<T, bool>> predicates) where T : new()
    {
        if (_dataProvider == null)
        {
            throw new InvalidOperationException("No data provider has been added. Call AddProvider() before generating data.");
        }

        if (!_dataMaps.TryGetValue(typeof(T), out var dataMap))
        {
            throw new InvalidOperationException($"No data map found for type {typeof(T).Name}. Call AddDataMap<{typeof(T).Name}>() to configure mappings.");
        }

        _addressCache.Clear();

        var map = (DataMap<T>)dataMap;
        var primaryTable = _dataProvider[map.PrimaryTableName];
        var rowCount = primaryTable.GetRowCount();

        if (rowCount == 0)
        {
            throw new InvalidOperationException($"Primary table '{map.PrimaryTableName}' has no rows.");
        }

        var yieldedCount = 0;
        var selectedRows = new HashSet<int>();
        var generationIndex = 0;

        // Phase 1: If predicates exist, scan all rows to find and yield matching items first
        if (predicates.Count > 0)
        {
            for (int rowIndex = 0; rowIndex < rowCount && yieldedCount < count; rowIndex++)
            {
                var primaryRow = primaryTable[rowIndex];
                var item = CreateItem<T>(map, primaryRow, _dataProvider, generationIndex);

                // Check if this item matches any predicate
                if (predicates.Any(p => p(item)))
                {
                    selectedRows.Add(rowIndex);
                    yieldedCount++;
                    generationIndex++;
                    yield return item;
                }
            }
        }

        // Phase 2: Fill remaining slots with other items based on selection strategy
        var sequentialIndex = 0;
        var maxAttempts = allowRepeats ? count * 100 : rowCount;
        var attempts = 0;

        while (yieldedCount < count && attempts < maxAttempts)
        {
            int rowIndex;
            if (strategy == SelectionStrategy.Sequential)
            {
                if (allowRepeats)
                {
                    rowIndex = sequentialIndex % rowCount;
                }
                else
                {
                    rowIndex = sequentialIndex;
                    if (rowIndex >= rowCount)
                    {
                        break;
                    }
                }
                sequentialIndex++;
            }
            else // Random
            {
                if (!allowRepeats && selectedRows.Count >= rowCount)
                {
                    break;
                }

                rowIndex = _random.Next(0, rowCount);
                var safetyCounter = 0;
                while (!allowRepeats && selectedRows.Contains(rowIndex) && safetyCounter < rowCount * 2)
                {
                    rowIndex = _random.Next(0, rowCount);
                    safetyCounter++;
                }

                if (!allowRepeats && selectedRows.Contains(rowIndex))
                {
                    break; // Couldn't find an unused row
                }
            }

            // Skip rows already selected in Phase 1
            if (!allowRepeats && selectedRows.Contains(rowIndex))
            {
                attempts++;
                continue;
            }

            var primaryRow = primaryTable[rowIndex];
            selectedRows.Add(rowIndex);

            var item = CreateItem<T>(map, primaryRow, _dataProvider, generationIndex);
            yieldedCount++;
            generationIndex++;
            yield return item;

            attempts++;
        }
    }

    /// <summary>
    /// Creates and populates an entity from a data row.
    /// </summary>
    private T CreateItem<T>(DataMap<T> map, IDataRow primaryRow, IDataProvider provider, int index) where T : new()
    {
        var item = new T();
        foreach (var mapping in map.Mappings)
        {
            var property = typeof(T).GetProperty(mapping.Key);
            if (property != null)
            {
                var value = mapping.Value.GetValue(primaryRow, provider, index);

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
                    property.SetValue(item, null);
                }
            }
        }
        return item;
    }

    /// <summary>
    /// Returns a randomly selected value from the specified enumeration type.
    /// </summary>
    /// <remarks>Each call to this method creates a new random value. The method is generic and can be used
    /// with any enumeration type.</remarks>
    /// <typeparam name="T">The enumeration type from which to select a random value. Must be an <see cref="Enum"/>.</typeparam>
    /// <returns>A randomly chosen value of type <typeparamref name="T"/>.</returns>
    public static T GetRandomEnum<T>()
        where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        var random = new Random();
        return (T)values.GetValue(random.Next(values.Length))!;
    }
}
