using Azure.Messaging.ServiceBus;
using AzureServiceBus.Sender.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AzureServiceBus.Sender.Controllers;

[ApiController]
[Route("[controller]")]
public class PizzaOrderController : Controller
{
    private const string ConnectionString = "";
    private const string QueueName = "pizzaqueue";
    
    [HttpPost]
    [Route("/PizzaOrder/CreateOrder", Name = "CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromForm] PizzaOrderForm orderForm)
    {
        //var pizzaOrders = orderForm.Name.Select((n, i) => new PizzaOrder { Name = n, Quantity = orderForm.Quantity[i] }).Where(q => q.Quantity > 1).ToList();

        var pizzasToOrder = new List<PizzaOrder>();
        for (int i = 0; i < orderForm.Name.Length; i++)
        {
            if (orderForm.Quantity[i] > 0)
            {
                pizzasToOrder.Add(new PizzaOrder
                {
                    Name = orderForm.Name[i],
                    Quantity = orderForm.Quantity[i]
                });
            }
        }

        var fakeOrderId = Guid.NewGuid().ToString()[..6];

        await ProcessOrder(pizzasToOrder);

        return View("OrderPlaced", fakeOrderId);
    }

    private async Task ProcessOrder(List<PizzaOrder> order)
    {
        // TODO: Should abstract this out to an interface e.g. IMessagePublisher
        
        await using var client = new ServiceBusClient(ConnectionString, 
            new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });

        await using var sender = client.CreateSender(QueueName);

        var messageToSend = new ServiceBusMessage(JsonConvert.SerializeObject(order));

        await sender.SendMessageAsync(messageToSend);
    }
}