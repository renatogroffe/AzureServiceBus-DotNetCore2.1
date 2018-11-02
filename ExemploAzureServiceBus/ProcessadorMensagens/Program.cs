using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Azure.ServiceBus;

namespace ProcessadorMensagens
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");
            var configuration = builder.Build();

            var serviceBusConfigurations = new ServiceBusConfigurations();
            new ConfigureFromConfigurationOptions<ServiceBusConfigurations>(
                configuration.GetSection("ServiceBusConfigurations"))
                    .Configure(serviceBusConfigurations);

            var client = new QueueClient(
                serviceBusConfigurations.ConnectionString,
                serviceBusConfigurations.QueueName,
                ReceiveMode.ReceiveAndDelete);
            try
            {
                client.RegisterMessageHandler(
                       async (message, token) =>
                       {
                           ProcessarMensagem(message);
                       },
                       new MessageHandlerOptions(
                           async (e) =>
                           {
                               Console.WriteLine("[Erro] " +
                                   e.Exception.GetType().FullName + " " +
                                   e.Exception.Message);
                           }
                       )
                );

                Console.ReadKey();
            }
            finally
            {
                client.CloseAsync().Wait();
            }
        }

        private static void ProcessarMensagem(
            Message message)
        {
            var conteudo = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine(Environment.NewLine +
                "[Nova mensagem recebida] " + conteudo);
        }
    }
}