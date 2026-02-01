using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace StudyDesigner.FuncCluedinProcessor;

public class CluedinProcessorFunction
{
    private readonly ILogger<CluedinProcessorFunction> _logger;

    public CluedinProcessorFunction(ILogger<CluedinProcessorFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(CluedinProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("questions", "cluedin-subscription", Connection = "servicebus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);

         // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}
