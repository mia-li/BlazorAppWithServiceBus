using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SBShared.Models;
using System.Text.Json;

namespace SBReceiver
{
    class Program
    {
        const string connectString = "Endpoint=sb://servicebusz.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=9mc2lhjeX/JBUDGp2BEKraoXZLGzWoWswDv6M1h12I4=";
        const string queueName = "personqueue";
        static IQueueClient queueClient;


        static async Task Main(string[] args)
        {
            queueClient = new QueueClient(connectString, queueName);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionreceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            //当queue中来消息了，会call 这个方法：ProcessMessageAsync
            queueClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);

            Console.ReadLine();
            await queueClient.CloseAsync();
        }

        private static Task ExceptionreceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Message handler exception:{arg.Exception}");
            return Task.CompletedTask;
        }

        private static async Task ProcessMessageAsync(Message message, CancellationToken token)
        {
            var jsonString = Encoding.UTF8.GetString(message.Body);
            PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString);
            Console.WriteLine($"Person Received:{person.FirstName}{person.LastName}");

            //remove the message from the queue
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        
    }
}
