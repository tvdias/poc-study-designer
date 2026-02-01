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
            _logger.LogInformation("Consuming study message - ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Handle the received study message
            // Add your consumption logic here

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
