using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TrafficLights.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        [HttpGet("all")]
        public IActionResult All()
        {
            return Ok("All access");
        }

        [HttpGet("admin")]
        [Authorize(Roles = "admin", Policy = "OnlyForAdmin")]
        public IActionResult Admin()
        {
            return Ok("admin access");
        }


        [HttpGet("user")]
        [Authorize(Roles = "user", Policy = "OnlyForUser")]
        public IActionResult User()
        {
            return Ok("User access");
        }
    }
}
