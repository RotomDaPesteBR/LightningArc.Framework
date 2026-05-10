using LightningArc.Results;
using LightningArc.Results.AspNetCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace LightningArc.Results.AspNetCore.Tests.Server.Controllers
{
    [ApiController]
    [EnableCors("AllowAll")]
    [Route("v1/[Controller]")]
    public class ResultsController(ILogger<ResultsController> logger) : Controller
    {
        private readonly ILogger<ResultsController> _logger = logger;
            
        [HttpGet]
        public EndpointResult<string> GetResult()
        {
            try
            {
                //Result.OrderProcessed(); 
                //Result.Success();

                //Result.Of.OrderProcessed();
                //Success.Of.OrderProcessed();

                //Error.Resource.NotFound(); 
                //Error.Business.OrderRejected();

                //var t = typeof(Success<string>.CreatedSuccess);

                //return Error.Application.Internal("Erro de teste", [ new ("Teste", "Erro de teste") ]);
                //return Error.Database.ConstraintViolation("Erro de teste", [ new ("Teste", "Erro de teste") ]);
                return Error
                    .Of<Business>()
                    .OrderRejected("Erro de teste", [new("Teste", "Erro de teste")]);
                //return Result.Success("Resultado", Success.Created("Criado com sucesso"));
            }
            catch (Exception exception)
            {
                _logger.LogError("{Message}", exception.Message);
                return Error.Application.Internal("Ocorreu um erro");
            }
        }

        [HttpGet("Teste")]
        public EndpointResult<string> GetTestResult()
        {
            int[] values = [1, 2, 3];
            Result<List<string>> result = values.Select(v => v.ToString()).ToList();

            return result.Bind(v => {

                string? value = v.FirstOrDefault();

                return value is not null
                    ? Result.Success(value)
                    : Error.Resource.NotFound($"N�o foi encontrado");
            });
        }

        [HttpGet("Custom/{id:int}")]
        public EndpointResult<string> GetTestResultById(int id)
        {
            Result<string> result;

            if (id == 0)
            {
                result = "Teste";
            }
            else if (id == 1)
            {
                result = Result.Created($"Teste {id} foi criado", $"Teste criado com sucesso");
            }
            else if (id >= 2)
            {
                result = Error.Database.ConnectionFailed("Falha na conexão do banco de dados");
            }
            else if (id == -1)
            {
                result = Error.Application.InvalidParameter(
                    "Id inválido",
                    new ErrorDetail("Id", id.ToString()),
                    new ErrorDetail("Code", (id + 1).ToString())
                );
            }
            else
            {
                result = Error.Application.Internal(); //$"Erro id {id}"
            }

            return result;
        }

        [HttpGet("Example")]

        public EndpointResult<string> Example() =>
            Result.Success("teste").WithContentType("text/plain");
        //Result.Success("{\"teste\": \"teste\"}").WithContentType("application/json"); //, "Exemplo bem sucedido"

        public class Product {}

        private Result<Product> GetProductById(int id)
        {
            return new Product();
        }

        [HttpGet("Products/{id:int}")]
        public EndpointResult<Product> GetProduct([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error.Validation.InvalidParameter(details: [new(nameof(id),"Must be bigger than 0")]);
            }

            Result<Product> result = GetProductById(id);

            return result;
        }
    }
}




