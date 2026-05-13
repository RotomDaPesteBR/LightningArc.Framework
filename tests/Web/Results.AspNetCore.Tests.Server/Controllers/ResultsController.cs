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
                return Error
                    .Of<Business>()
                    .OrderRejected("Erro de teste", [new("Teste", "Erro de teste")]);
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

            return result.Bind(v =>
            {
                string? value = v.FirstOrDefault();

                return value is not null
                    ? Result.Success(value)
                    : Error.Resource.NotFound($"Não foi encontrado");
            });
        }

        [HttpGet("Test/{id:int}")]
        public EndpointResult<string> GetTestResultById(int id)
        {
            Result<string> result = id switch
            {
                -1 => Error.Application.InvalidParameter(
                    "Id inválido",
                    new ErrorDetail("Id", id.ToString()),
                    new ErrorDetail("Code", (id + 1).ToString())
                ),
                0 => "Teste",
                1 => Result.Created($"Teste {id} foi criado", $"Teste criado com sucesso"),
                >= 2 => Error.Database.ConnectionFailed("Falha na conexão do banco de dados"),
                _ => Error.Application.Internal(),
            };

            return result;
        }

        [HttpGet("Custom/{id:int}")]
        public EndpointResult<string> GetCustomResultById(int id)
        {
            Result<string> result = id switch
            {
                -1 => Error.Business.OrderRejected(
                    "Id inválido",
                    new ErrorDetail("Id", id.ToString()),
                    new ErrorDetail("Code", (id + 1).ToString())
                ),
                0 => Success<string>.Of.OrderProcessed("Test"),
                1 => Result.OrderProcessed($"Teste {id} foi criado", $"Teste criado com sucesso"),
                >= 2 => Result<string>.Of.OrderProcessed("Falha na conexão do banco de dados"),
                _ => Error.Application.Internal(),
            };

            return result;
        }

        [HttpGet("Example")]
        public EndpointResult<string> Example() =>
            Result.Success("teste").WithContentType("text/plain");

        //Result.Success("{\"teste\": \"teste\"}").WithContentType("application/json"); //, "Exemplo bem sucedido"

        public class Product { }

        private Result<Product> GetProductById(int id)
        {
            return new Product();
        }

        [HttpGet("Products/{id:int}")]
        public EndpointResult<Product> GetProduct([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error.Validation.InvalidParameter(
                    details: [new(nameof(id), "Must be bigger than 0")]
                );
            }

            Result<Product> result = GetProductById(id);

            return result;
        }
    }
}
