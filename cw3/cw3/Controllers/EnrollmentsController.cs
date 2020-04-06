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
            Enrollment response;
            try
            {
                response = _dbService.AddStudent(student);
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return Created("Created", response);
        }

        [HttpPost("promotions")]
        public IActionResult promotions(Promotion promotion)
        {
            Enrollment response;
            try
            {
                response = _dbService.Promotions(promotion);
            }catch(Exception e)
            {
                return NotFound(e.Message);
            }
            return Created("Created", response);
        }
    }
}