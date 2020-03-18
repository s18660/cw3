using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3.DAL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cw3
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("getStudentsFromDb")]
        public IActionResult GetStudentsFromDb()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("getStudents")]
        public string GetStudents()
        {
            return "Nowak, Kowalski";
        }

        [HttpGet("getStudent/{id}")]
        public IActionResult GetStudentId(int id)
        {
            if(id == 1)
            {
                return Ok("Norbert Gierczak");
            }
            else
            {
                return NotFound("Brak studenta o podanym id");
            }
        }

        [HttpGet]
        public string GetStudentsQuery(string orderBy)
        {
            return $"mój ziomo coś wygrał orderby={orderBy}";
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult ResponseToPut(int id)
        {
            return Ok("Aktualizacja dokończona: " + id);
        }

        [HttpDelete("{id}")]
        public IActionResult ResponseToDelete(int id)
        {
            return Ok("Usuwanie ukończone: " + id);
        }
    }
}
