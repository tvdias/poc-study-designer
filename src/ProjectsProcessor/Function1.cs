using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace StudyDesigner.FuncProjectsProcessor;

public class ProjectsProcessorFunction
{
    private readonly ILogger<ProjectsProcessorFunction> _logger;

    public ProjectsProcessorFunction(ILogger<ProjectsProcessorFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProjectsProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("projects", "projects-subscription", Connection = "servicebus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);

         // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}
