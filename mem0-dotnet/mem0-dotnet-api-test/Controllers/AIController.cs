using mem0_dotnet;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace mem0_dotnet_api_test.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AIController : ControllerBase
    {


        private readonly ILogger<AIController> _logger;
        private readonly Mem0Service _mem0Service;

        public AIController(ILogger<AIController> logger, Mem0Service mem0Service)
        {
            _logger = logger;
            _mem0Service = mem0Service;
        }

        [HttpPost]
        public async Task<string> SearchMemory(request request)
        {
            var result = await _mem0Service.SearchMemory(request.userId, request.query);

            return result[0].Payload["data"].StringValue;
        }

        [HttpPost]
        public async Task SaveMemory(request request)
        {
            await _mem0Service.SaveMemory(request.userId, request.query);
        }

        [NotMapped]
        public class request
        {

            public string query { get; set; }
            public string userId { get; set; }
        }
    }
}
