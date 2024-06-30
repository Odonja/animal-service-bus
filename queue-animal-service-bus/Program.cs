using Azure.Messaging.ServiceBus;

namespace queue_animal_service_bus;

class Program
{
    // set the environment variable with: setx AZURE_SERVICE_BUS_CONNECTION_STRING "<the connection string>"
    private static readonly string? connectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING");
    private static readonly string queueName = "configurationqueue";

    static async Task Main(string[] args)
    {
        if (connectionString == null)
        {
            Console.WriteLine("No animals can be delivered without a connectionString\n"
            + "please set the environment variable with: setx AZURE_SERVICE_BUS_CONNECTION_STRING \" < the connection string > \"");
            return;
        }
        var animal = RetrieveAnimalRequest();
        if (animal == null)
        {
            return;
        }
        await using var client = new ServiceBusClient(connectionString);
        await Task.WhenAll(SendAnimalFromStore(client, animal), UnloadAndHandoverAnimal(client));
    }

    private static async Task SendAnimalFromStore(ServiceBusClient client, String animal)
    {
        var sender = client.CreateSender(queueName);
        var cagedAnimal = new ServiceBusMessage(animal);
        await sender.SendMessageAsync(cagedAnimal);
    }

    private static async Task UnloadAndHandoverAnimal(ServiceBusClient client)
    {
        var receiver = client.CreateReceiver(queueName);
        var receivedAnimal = await receiver.ReceiveMessageAsync();
        var animal = receivedAnimal.Body.ToString();
        Console.WriteLine($"Here is your adorable {animal}");
    }

    private static string? RetrieveAnimalRequest()
    {
        Console.WriteLine("Which animal would you like the bus to deliver?\n"
            + "Press 1 for: Guinea pig\n"
            + "Press 2 for: Horse\n"
            + "Press 3 for: Dog\n"
            + "Press 4 for: Cat\n"
            + "Press 5 for: Bunny\n"
            + "Press 6 for: Grasshopper\n");

        var animalRequest = Console.ReadLine();
        string? animal = null;
        switch (animalRequest)
        {
            case "1":
                animal = "guinea pig";
                break;
            case "2":
                animal = "horse";
                break;
            case "3":
                animal = "dog";
                break;
            case "4":
                animal = "cat";
                break;
            case "5":
                animal = "bunny";
                break;
            case "6":
                animal = "grasshopper";
                break;
            default:
                Console.WriteLine($"There is no animal connected to number {animalRequest}, no animal delivery will be made.");
                break;
        }

        if (animal != null)
        {
            Console.WriteLine($"You selected: {animal}");
        }

        return animal;
    }
}
