using System;

public class SnowflakeIdGenerator
{
    // Custom epoch (e.g., Jan 1, 2020 UTC)
    private static readonly DateTime Epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int WorkerIdBits = 5;
    private const int DatacenterIdBits = 5;
    private const int SequenceBits = 12;

    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);         // 31
    private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits); // 31
    private const long SequenceMask = -1L ^ (-1L << SequenceBits);        // 4095

    private const int WorkerIdShift = SequenceBits; // 12
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits; // 17
    private const int TimestampShift = SequenceBits + WorkerIdBits + DatacenterIdBits; // 22

    private long _lastTimestamp = -1L;
    private long _sequence = 0L;

    public long WorkerId { get; }
    public long DatacenterId { get; }

    private readonly object _lock = new object();

    public SnowflakeIdGenerator(long workerId, long datacenterId)
    {
        if (workerId > MaxWorkerId || workerId < 0)
            throw new ArgumentException($"Worker Id must be between 0 and {MaxWorkerId}");

        if (datacenterId > MaxDatacenterId || datacenterId < 0)
            throw new ArgumentException($"Datacenter Id must be between 0 and {MaxDatacenterId}");

        WorkerId = workerId;
        DatacenterId = datacenterId;
    }

    public long NextId()
    {
        lock (_lock)
        {
            var timestamp = CurrentTimeMillis();
            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException(
                    $"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp}ms");
            }

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    // Sequence exhausted, wait until next millisecond
                    timestamp = WaitNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0L;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - ToUnixMillis(Epoch)) << TimestampShift) |
                   (DatacenterId << DatacenterIdShift) |
                   (WorkerId << WorkerIdShift) |
                   _sequence;
        }
    }

    private static long CurrentTimeMillis()
    {
        return (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
    }

    private static long ToUnixMillis(DateTime dt)
    {
        return (long)(dt - DateTime.UnixEpoch).TotalMilliseconds;
    }

    private static long WaitNextMillis(long lastTimestamp)
    {
        var ts = CurrentTimeMillis();
        while (ts <= lastTimestamp)
        {
            ts = CurrentTimeMillis();
        }
        return ts;
    }
}
