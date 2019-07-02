using System;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Messaging
{
    public class RabbitMQMessagingService : IMessagingService, IDisposable
    {
        private readonly RabbitMQConfig config;
        private IConnection connection;
        private IModel channel;

        public RabbitMQMessagingService(RabbitMQConfig config)
        {
            this.config = config;
        }

        public Task Send(string topic, object obj)
        {
            return Task.Run(() =>
            {
                var factory = this.GetConnectionFactory();
                using(var connection = factory.CreateConnection())
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: topic,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var msg = JsonConvert.SerializeObject(obj);
                    var body = Encoding.UTF8.GetBytes(msg);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: topic,
                        basicProperties: null,
                        body: body);
                }
            });
        }

        public Task RegisterHandler<T>(string topic, Action<T> handler)
        {
            var factory = this.GetConnectionFactory();
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            this.channel.QueueDeclare(
                queue: topic,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(this.channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var obj = JsonConvert.DeserializeObject<T>(message);
                handler(obj);
            };
            this.channel.BasicConsume(
                queue: topic,
                autoAck: true,
                consumer: consumer);
            return Task.CompletedTask;
        }

        private ConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory()
            {
                HostName = this.config.HostName,
                Port = this.config.Port,
                UserName = this.config.User,
                Password = this.config.Password
            };
        }

        public void Dispose()
        {
            this.channel.Dispose();
            this.connection.Dispose();
        }
    }
}