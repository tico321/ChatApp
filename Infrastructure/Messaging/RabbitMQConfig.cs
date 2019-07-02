namespace Infrastructure.Messaging
{
    public class RabbitMQConfig
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}