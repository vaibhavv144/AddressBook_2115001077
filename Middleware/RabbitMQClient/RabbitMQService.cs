using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Email;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Middleware.RabbitMQClient
{
    public class RabbitMQService : IRabbitMQService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _hostname = "localhost";  // Change if needed
        private readonly string _queueName = "AddressBook"; // Queue name
        private readonly SMTP _smtp;
        public RabbitMQService(SMTP _smtp)
        {
         
            this._smtp = _smtp;
        }

        public void SendMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

                _logger.Info("Message sent to queue: {0}", message);
            }
        }

        public void ReceiveMessage()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.Info("Received message from queue: {0}", message);

                // ✅ Extract email & message
                var parts = message.Split(',', 2); // Splitting at first comma
                if (parts.Length == 2)
                {
                    string email = parts[0].Trim();
                    string emailMessage = parts[1].Trim();

                    _logger.Info("Extracted Email: {0}, Message: {1}", email, emailMessage);

                    // ✅ Send email
                    await _smtp.SendEmailAsync(email, "Welcome to AddressBook", emailMessage);
                }
                else
                {
                    _logger.Error("Invalid message format: {0}", message);
                }
            };

            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
    }
}
