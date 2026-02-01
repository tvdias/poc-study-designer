using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace StudyProcessor
{
    public class ProcessStudyFunction
    {
        private readonly ILogger<ProcessStudyFunction> _logger;

        public ProcessStudyFunction(ILogger<ProcessStudyFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessStudyFunction))]
        public async Task Run(
            [ServiceBusTrigger("sampletopic", "processor-subscription", Connection = "sb")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("=== StudyProcessor: Hello World ===");
            _logger.LogInformation("Processing message - ID: {id}", message.MessageId);
            _logger.LogInformation("Study data: {body}", message.Body);

            // Simple hello world processing
            _logger.LogInformation("Study processing complete!");

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
