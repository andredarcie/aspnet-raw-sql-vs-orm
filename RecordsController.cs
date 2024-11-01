using Microsoft.AspNetCore.Mvc;
using desafio.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        private readonly ILogger<RecordsController> _logger;
        private readonly RecordsService _recordsService;

        public RecordsController(ILogger<RecordsController> logger, RecordsService recordsService)
        {
            _logger = logger;
            _recordsService = recordsService;
        }

        [HttpPost("create")]
        public IActionResult CreateRecord()
        {
            try
            {
                var random = new Random();
                var name = $"User{random.Next(1, 1000)}";
                var age = random.Next(18, 100);

                var newId = _recordsService.CreateRecord(name, age);
                
                return Ok(new { Id = newId, Message = "Record created", Name = name, Age = age });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar um novo registro.");
                return Problem("Erro ao criar o registro.");
            }
        }

        [HttpGet("read")]
        public IActionResult ReadLastRecord()
        {
            try
            {
                var lastRecord = _recordsService.GetLastRecord();
                if (lastRecord.HasValue)
                {
                    var (id, name, age) = lastRecord.Value;
                    return Ok(new { Id = id, Name = name, Age = age });
                }

                return NotFound("No record found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler o último registro.");
                return Problem("Erro ao ler o registro.");
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck() => Ok("Healthy");
    }
}
