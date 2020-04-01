using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3.Data;
using cw3.Services;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;

        public EnrollmentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost("addStudent")]
        public IActionResult addStudent(Student student)
        {
            var response = _dbService.addStudent(student);
            if(response == null)
            {
                return BadRequest();
            }
            return Created("Created", response);
        }

        [HttpPost("promotions")]
        public IActionResult promotions(Promotion promotion)
        {
            var response = _dbService.promotions(promotion);
            if(response == null)
            {
                return NotFound();
            }
            return Created("", response);
        }
    }
}