using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connection factory
            var factory = new ConnectionFactory() { HostName = "localhost" };

            // Create connection
            using (var connection = factory.CreateConnection())
            {
                // Create channel
                using (var channel = connection.CreateModel())
                {
                    // Declare exchange
                    channel.ExchangeDeclare("food_order_exchange", ExchangeType.Direct);

                    // Declare queues
                    for (int i = 1; i <= 3; i++)
                    {
                        channel.QueueDeclare($"food_order_queue_{i}", durable: true, exclusive: false, autoDelete: false, arguments: null);
                        channel.QueueBind($"food_order_queue_{i}", "food_order_exchange", $"food_order_routing_key_{i}");
                    }

                    // Create multiple producers
                    for (int i = 1; i <= 2; i++)
                    {
                        Task.Run(() =>
                        {
                            using (var producerChannel = connection.CreateModel())
                            {
                                var producerQueueName = $"food_order_queue_{i}";

                                while (true)
                                {
                                    Console.WriteLine($"Producer {i} is sending an order");

                                    var message = $"Order from producer {i} at {DateTime.Now}";

                                    var body = Encoding.UTF8.GetBytes(message);

                                    producerChannel.BasicPublish("food_order_exchange", $"food_order_routing_key_{i}", null, body);

                                    Task.Delay(5000).Wait();
                                }
                            }
                        });
                    }

                    // Create multiple consumers
                    for (int i = 1; i <= 3; i++)
                    {
                        Task.Run(() =>
                        {
                            using (var consumerChannel = connection.CreateModel())
                            {
                                var consumerQueueName = $"food_order_queue_{i}";

                                var consumer = new EventingBasicConsumer(consumerChannel);

                                consumer.Received += (model, ea) =>
                                {
                                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                                    Console.WriteLine($"Consumer {i} received an order: {message}");

                                    consumerChannel.BasicAck(ea.DeliveryTag, multiple: false);
                                };

                                consumerChannel.BasicConsume(consumerQueueName, autoAck: false, consumer: consumer);
                            }
                        });
                    }

                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                }
            }
        }
    }
}
