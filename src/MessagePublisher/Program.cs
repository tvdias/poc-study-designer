using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace MessagePublisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["ServiceBus:ConnectionString"] 
                ?? Environment.GetEnvironmentVariable("sb") 
                ?? throw new InvalidOperationException("Service Bus connection string not configured. Set 'ServiceBus:ConnectionString' in appsettings.json or 'sb' environment variable.");
            
            var topicName = configuration["ServiceBus:TopicName"] ?? "sampletopic";

            Console.WriteLine("==============================================");
            Console.WriteLine("Study Designer - Message Publisher");
            Console.WriteLine("Hello World Console App");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            await using var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(topicName);

            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Send a hello world message");
                Console.WriteLine("2. Exit");
                Console.Write("\nSelect an option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await SendHelloWorldMessage(sender);
                        break;
                    case "2":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static async Task SendHelloWorldMessage(ServiceBusSender sender)
        {
            var messageContent = $"Hello World from Study Designer! Sent at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

            var message = new ServiceBusMessage(messageContent)
            {
                ContentType = "text/plain",
                MessageId = Guid.NewGuid().ToString(),
                Subject = "HelloWorld"
            };

            await sender.SendMessageAsync(message);
            Console.WriteLine($"\n✓ Message sent successfully!");
            Console.WriteLine($"  Message ID: {message.MessageId}");
            Console.WriteLine($"  Content: {messageContent}");
        }
    }
}
