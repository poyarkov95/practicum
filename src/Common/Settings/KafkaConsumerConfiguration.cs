namespace Common.Settings;

public class KafkaConsumerConfiguration : KafkaConfiguration
{
    public string GroupId { get; set; }
}