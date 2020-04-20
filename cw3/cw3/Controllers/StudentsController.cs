using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using cw3.DAL;
using cw3.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace cw3.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private readonly IConfiguration Configuration;

        public StudentsController(IConfiguration configuration, IDbService dbService)
        {
            Configuration = configuration;
            _dbService = dbService;
        }

        [HttpGet("getStudentsFromDb")]
        public IActionResult GetStudentsFromDb()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("getStudentEnrollment/{id}")]
        public IActionResult getStudentEnrollment(string id)
        {
            return Ok(_dbService.GetStudentEnrollment(id));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequestDto request)
        {
            if(!_dbService.CheckCredentials(request))
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Login),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    issuer: "s18660",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: creds
                );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });
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
