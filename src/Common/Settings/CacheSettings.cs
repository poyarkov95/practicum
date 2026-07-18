namespace Common.Settings;

public class CacheSettings
{
    /// <summary>
    /// TTL для события в кэше
    /// </summary>
    public int EventCacheTTLSeconds { get; set; }
    
    /// <summary>
    /// TTL для топ-10 событий в кэше
    /// </summary>
    public int TopEventsCacheTTLSeconds { get; set; }
}