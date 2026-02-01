using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusConsumer
{
    public class ConsumeStudyMessageFunction
    {
        private readonly ILogger<ConsumeStudyMessageFunction> _logger;

        public ConsumeStudyMessageFunction(ILogger<ConsumeStudyMessageFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ConsumeStudyMessageFunction))]
        public async Task Run(
            [ServiceBusTrigger("sampletopic", "designer-subscription", Connection = "sb")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("=== ServiceBusConsumer: Hello World ===");
            _logger.LogInformation("Message received - ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);

            // Simple hello world processing
            _logger.LogInformation("Processing complete!");

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
