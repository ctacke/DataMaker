namespace DataMaker;

/// <summary>
/// Generates deterministic IDs for testing purposes.
/// When initialized with the same seed, produces the same sequence of IDs.
/// </summary>
public class IdGenerator
{
    private Random _random;
    private int _workerId;
    private long _sequence;

    /// <summary>
    /// Creates a new IdGenerator with the specified seed for deterministic generation.
    /// </summary>
    /// <param name="seed">The seed value. Same seed produces same ID sequence.</param>
    public IdGenerator(int seed)
    {
        _random = new Random(seed);
        _workerId = _random.Next(0, 1024); // 10 bits for worker ID component
        _sequence = 0;
    }

    /// <summary>
    /// Generates the next long ID in the sequence.
    /// Produces Snowflake-like 64-bit IDs that are deterministic based on seed.
    /// </summary>
    /// <returns>A deterministic long ID.</returns>
    public long NextLong()
    {
        // Snowflake-like structure:
        // - 41 bits: timestamp-like value (derived from sequence for determinism)
        // - 10 bits: worker ID (derived from seed)
        // - 12 bits: sequence counter

        // Base timestamp representing ~2023, plus sequence offset
        // This produces IDs that look like real Snowflake IDs
        var baseTimestamp = 1700000000000L; // ~Nov 2023 in milliseconds
        var timestamp = baseTimestamp + _sequence;

        // Ensure timestamp fits in 41 bits (max ~69 years from epoch)
        timestamp &= 0x1FFFFFFFFFFL;

        var workerId = (long)(_workerId & 0x3FF); // 10 bits
        var seq = _sequence & 0xFFF; // 12 bits

        _sequence++;

        // Combine: timestamp << 22 | workerId << 12 | sequence
        return (timestamp << 22) | (workerId << 12) | seq;
    }

    /// <summary>
    /// Generates the next int ID in the sequence.
    /// Produces positive integers that are deterministic based on seed.
    /// </summary>
    /// <returns>A deterministic positive int ID.</returns>
    public int NextInt()
    {
        return _random.Next(1, int.MaxValue);
    }

    /// <summary>
    /// Generates the next Guid in the sequence.
    /// Produces deterministic GUIDs based on seed - same seed, same sequence of GUIDs.
    /// </summary>
    /// <returns>A deterministic Guid.</returns>
    public Guid NextGuid()
    {
        var bytes = new byte[16];
        _random.NextBytes(bytes);

        // Set version to 4 (random) and variant to RFC 4122
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40); // Version 4
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // Variant RFC 4122

        return new Guid(bytes);
    }

    /// <summary>
    /// Generates the next string ID in the sequence.
    /// Produces alphanumeric strings that are deterministic based on seed.
    /// </summary>
    /// <param name="length">Length of the random portion (default 8).</param>
    /// <param name="prefix">Optional prefix for the ID (e.g., "USR-", "ORD-").</param>
    /// <returns>A deterministic string ID.</returns>
    public string NextString(int length = 8, string? prefix = null)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[_random.Next(chars.Length)];
        }
        return prefix == null ? new string(result) : $"{prefix}{new string(result)}";
    }

    /// <summary>
    /// Generates a sequence of long IDs.
    /// </summary>
    /// <param name="count">Number of IDs to generate.</param>
    /// <returns>An enumerable of deterministic long IDs.</returns>
    public IEnumerable<long> TakeLong(int count)
    {
        for (int i = 0; i < count; i++)
            yield return NextLong();
    }

    /// <summary>
    /// Generates a sequence of int IDs.
    /// </summary>
    /// <param name="count">Number of IDs to generate.</param>
    /// <returns>An enumerable of deterministic int IDs.</returns>
    public IEnumerable<int> TakeInt(int count)
    {
        for (int i = 0; i < count; i++)
            yield return NextInt();
    }

    /// <summary>
    /// Generates a sequence of Guids.
    /// </summary>
    /// <param name="count">Number of GUIDs to generate.</param>
    /// <returns>An enumerable of deterministic GUIDs.</returns>
    public IEnumerable<Guid> TakeGuid(int count)
    {
        for (int i = 0; i < count; i++)
            yield return NextGuid();
    }

    /// <summary>
    /// Generates a sequence of string IDs.
    /// </summary>
    /// <param name="count">Number of IDs to generate.</param>
    /// <param name="length">Length of each ID's random portion.</param>
    /// <param name="prefix">Optional prefix for each ID.</param>
    /// <returns>An enumerable of deterministic string IDs.</returns>
    public IEnumerable<string> TakeString(int count, int length = 8, string? prefix = null)
    {
        for (int i = 0; i < count; i++)
            yield return NextString(length, prefix);
    }

    /// <summary>
    /// Resets the generator to its initial state, allowing the same sequence to be generated again.
    /// </summary>
    /// <param name="seed">The seed value to reset to.</param>
    public void Reset(int seed)
    {
        _random = new Random(seed);
        _workerId = _random.Next(0, 1024);
        _sequence = 0;
    }
}
