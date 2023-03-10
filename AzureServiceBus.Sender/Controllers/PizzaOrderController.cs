using Azure.Messaging.ServiceBus;
using AzureServiceBus.Sender.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AzureServiceBus.Sender.Controllers;

[ApiController]
[Route("[controller]")]
public class PizzaOrderController : Controller
{
    // TODO: Should use configuration for these
    private const string ConnectionString = "";
    private const string QueueName = "pizzaqueue";
    
    [HttpPost]
    [Route("/PizzaOrder/CreateOrder", Name = "CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromForm] PizzaOrderForm orderForm)
    {
        var pizzaOrders = orderForm.Name.Select((n, i) => new PizzaOrder { Name = n, Quantity = orderForm.Quantity[i] }).Where(q => q.Quantity > 1);

        await ProcessOrder(pizzaOrders);

        var fakeOrderId = Guid.NewGuid().ToString()[..6];

        return View("OrderPlaced", fakeOrderId);
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