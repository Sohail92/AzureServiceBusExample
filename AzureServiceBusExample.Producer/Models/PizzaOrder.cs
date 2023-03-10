namespace AzureServiceBusExample.Producer.Models
{
    public class PizzaOrder
    {
        public string Name { get; set; }

        public string[] Toppings { get; set; }
    }
}
