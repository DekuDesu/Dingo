using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Authenticate : ControllerBase
    {
        private readonly ILogger<Authenticate> _logger;

        public Authenticate(ILogger<Authenticate> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public void Get()
        {

        }

        [HttpPost]
        public void Post()
        {

        }
    }
}
