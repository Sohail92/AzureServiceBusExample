using Azure.Messaging.ServiceBus;
using AzureServiceBusExample.Producer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AzureServiceBusExample.Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class PizzaOrderController : ControllerBase
{
    // TODO: Should use configuration for these
    private const string ConnectionString = "Endpoint=sb://azureservicebusexampleproducer.servicebus.windows.net/;SharedAccessKeyName=SohailsPolicy;SharedAccessKey=TRF7NW+0yTsQU/RBV2eaIrH5JSO9+2HVn+ASbI213b8=;EntityPath=pizzaqueue";
    private const string QueueName = "pizzaqueue";
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] IEnumerable<PizzaOrder> orders)
    {
        await ProcessOrder(orders);
        return Ok();
    }

    private async Task ProcessOrder(IEnumerable<PizzaOrder> orders)
    {
        // TODO: Should abstract this out to an interface e.g. IMessagePublisher
        
        await using var client = new ServiceBusClient(ConnectionString, 
            new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });

        await using var sender = client.CreateSender(QueueName);

        var messageToSend = new ServiceBusMessage(JsonConvert.SerializeObject(orders));

        await sender.SendMessageAsync(messageToSend);
    }
}

// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.