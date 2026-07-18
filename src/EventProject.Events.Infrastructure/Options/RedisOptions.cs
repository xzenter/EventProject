namespace EventProject.Events.Infrastructure.Options
{
    public class RedisOptions
    {
        public string RedisServers { get; set; } = "localhost:6379";
        public string? Password { get; set; }
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 3000;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectRetry { get; set; } = 3; 
    }
}
