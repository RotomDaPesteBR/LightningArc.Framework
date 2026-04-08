using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using LightningArc.Results.AspNetCore.Extensions;
using LightningArc.Results;
using LightningArc.Results.AspNetCore;

namespace LightningArc.Results.AspNetCore.Tests.Server.Controllers
{
    [ApiController]
    [Route("v1/[Controller]")]
    public class SettingsController(ILogger<SettingsController> logger, IOutputCacheStore cache) : Controller
    {
        private readonly ILogger<SettingsController> _logger = logger;
        private readonly IOutputCacheStore _cache = cache;

        [HttpPost("Caches/Clear")]
        [EnableCors("AllowAll")]
        public async Task<EndpointResult<string>> ClearCaches()
        {
            try
            {
                await _cache.EvictByTagAsync("All", CancellationToken.None);

                return Result.Success("Cache de toda aplicação limpo com sucesso.");
            }
            catch (Exception exception)
            {
                _logger.LogError("{message}", exception.Message);
                return Error.Application.Internal("Erro ao recarregar o cache, tente novamente mais tarde;");
            }
        }


        [HttpGet("Teste")]
        [EnableCors("AllowAll")]
        public async Task<EndpointResult<string>> Teste()
        {
            try
            {
                await _cache.EvictByTagAsync("All", CancellationToken.None);

                return Result.Created("Cache de toda aplicação limpo com sucesso.").WithContentType("text/plain");
            }
            catch (Exception exception)
            {
                _logger.LogError("{message}", exception.Message);
                return Error.Application.Internal("Erro ao recarregar o cache, tente novamente mais tarde;");
            }
        }

        [HttpPost("Caches/Clear/{tag}")]
        [EnableCors("AllowAll")]
        public async Task<EndpointResult<string>> ClearCaches([FromRoute] string tag)
        {
            try
            {
                await _cache.EvictByTagAsync(tag, CancellationToken.None);

                return Result.Success($"Caches com a tag {tag} limpo com sucesso.");
            }
            catch (Exception exception)
            {
                _logger.LogError("{message}", exception.Message);
                return Error.Application.Internal($"Erro ao recarregar o cache tag {tag}, tente novamente mais tarde;");
            }
        }
    }
}




