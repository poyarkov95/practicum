namespace Common.Settings;

public class KafkaConfiguration
{
    public string BootstrapServers { get; set; }
    public string BookingCreatedTopic { get; set; }
    public string BookingCancelledTopic { get; set; }
    public string BookingProcessedSuccessfullyTopic { get; set; }
    public string BookingProcessedUnsuccessfullyTopic { get; set; }
}