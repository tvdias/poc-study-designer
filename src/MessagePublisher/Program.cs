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

            Console.WriteLine("Study Designer - Service Bus Message Publisher");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            await using var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(topicName);

            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Send a test message");
                Console.WriteLine("2. Send a study creation message");
                Console.WriteLine("3. Exit");
                Console.Write("\nSelect an option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await SendTestMessage(sender);
                        break;
                    case "2":
                        await SendStudyMessage(sender);
                        break;
                    case "3":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static async Task SendTestMessage(ServiceBusSender sender)
        {
            Console.Write("Enter message content: ");
            var content = Console.ReadLine() ?? "Test message";

            var message = new ServiceBusMessage(content)
            {
                ContentType = "text/plain",
                MessageId = Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(message);
            Console.WriteLine($"✓ Test message sent successfully! Message ID: {message.MessageId}");
        }

        static async Task SendStudyMessage(ServiceBusSender sender)
        {
            Console.Write("Enter study name: ");
            var studyName = Console.ReadLine() ?? "Sample Study";

            Console.Write("Enter study description: ");
            var studyDescription = Console.ReadLine() ?? "Sample Description";

            var studyData = System.Text.Json.JsonSerializer.Serialize(new
            {
                StudyId = Guid.NewGuid().ToString(),
                StudyName = studyName,
                Description = studyDescription,
                CreatedAt = DateTime.UtcNow,
                Status = "Draft"
            });

            var message = new ServiceBusMessage(studyData)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Subject = "StudyCreated"
            };

            message.ApplicationProperties.Add("MessageType", "StudyCreated");

            await sender.SendMessageAsync(message);
            Console.WriteLine($"✓ Study message sent successfully! Message ID: {message.MessageId}");
            Console.WriteLine($"  Study data: {studyData}");
        }
    }
}
