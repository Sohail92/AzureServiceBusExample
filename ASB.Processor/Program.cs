using System.Text;
using Azure.Messaging.ServiceBus;
using AzureServiceBus.Sender.Models;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using SendGrid;

string serviceBusConString =
    "";
string queueName = "pizzaqueue";

await using var client = new ServiceBusClient(serviceBusConString,
    new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    });

ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string msgBody = args.Message.Body.ToString();
    Console.WriteLine($"Received: {msgBody}");

    await SendOrderAcceptedEmail(msgBody);

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

static async Task SendOrderAcceptedEmail(string msgBody)
{
    var pizzaOrderDetails = JsonConvert.DeserializeObject<List<PizzaOrder>>(msgBody);

    var apiKey = "";
    var client = new SendGridClient(apiKey);
    var from = new EmailAddress("contact@sohailrahman.net", "CodeDojoTakeaway-EmailSender");
    var subject = "Pizza on its way!";
    var to = new EmailAddress("sohail.rahman@bglgroup.co.uk", "CodeDojoTakeaway-EmailReceiver");
    var plainTextContent = "Thanks for your order!";
    
    StringBuilder emailHTML = new StringBuilder("<h1>Order Accepted!</h1>");

    emailHTML.Append("<b>Your order<b>");

    foreach (var pizza in pizzaOrderDetails)
    {
        emailHTML.Append($"<p>{pizza.Name}</p>");
        emailHTML.Append($"<p>Quantity:{pizza.Quantity}</p>");
        emailHTML.Append("<hr>");
    }

    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, emailHTML.ToString());
    
    var response = await client.SendEmailAsync(msg);

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Email request sent"
        : $"Email request failed with status code: {response.StatusCode}");
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}