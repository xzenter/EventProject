namespace EventProject.Events.Infrastructure.Options;

public class KafkaOptions
{
    public string BootstrapServers { get; set; }
    public string ConsumerGroup { get; set; }
}