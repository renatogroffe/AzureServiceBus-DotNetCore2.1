using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using APIMensagens.Models;

namespace APIMensagens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensagensController : ControllerBase
    {
        private static Contador _CONTADOR = new Contador();

        [HttpGet]
        public object Get()
        {
            return new
            {
                QtdMensagensEnviadas = _CONTADOR.ValorAtual
            };
        }

        [HttpPost]
        public object Post(
            [FromServices]ServiceBusConfigurations configurations,
            [FromBody]Conteudo conteudo)
        {
            lock (_CONTADOR)
            {
                _CONTADOR.Incrementar();

                string message =
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} - " +
                    $"Conteúdo da Mensagem: {conteudo.Mensagem}";
                var body = Encoding.UTF8.GetBytes(message);

                var client = new QueueClient(
                    configurations.ConnectionString,
                    configurations.QueueName,
                    ReceiveMode.ReceiveAndDelete);
                client.SendAsync(new Message(body)).Wait();

                return new
                {
                    Resultado = "Mensagem encaminhada com sucesso"
                };
            }
        }
    }
}