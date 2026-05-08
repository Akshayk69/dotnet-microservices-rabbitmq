namespace CommandService.Controllers
{
    using System;
    using AutoMapper;
    using CommandService.Data;
    using CommandService.Dtos;
    // using System.Collections.Generic;
    // using System.Linq;
    // using System.Threading.Tasks;
    // using AutoMapper;
    // using CommandService.Data;
    // using CommandService.Dtos;
    // using CommandService.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public PlatformsController(ICommandRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("--> Getting Platforms from command service...");

            var platformItems = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpPost]
        public ActionResult TestInboundConnection()
        {
            Console.WriteLine("--> Inbound test # Command Service");

            return Ok("Inbound test of from Platforms Controller");
        }
    }
}