using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using cw3.DTOs;
using cw3.Services;
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
        private readonly IStudentsDbService _dbService;
        private readonly IConfiguration Configuration;

        public StudentsController(IConfiguration configuration, IStudentsDbService dbService)
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
        public IActionResult LoginStudent(LoginRequestDto request)
        {
            if(!_dbService.CheckCredentials(request))
            {
                return Unauthorized();
            }

            return Ok(GenerateToken(request.Login));
        }

        [HttpPost("refreshToken/{refreshToken}")]
        [AllowAnonymous]
        public IActionResult RefreshToken(string refreshToken)
        {
            string login = _dbService.CheckRefreshToken(refreshToken);
            if (login == "")
            {
                return Unauthorized();
            }

            return Ok(GenerateToken(login));
        }

        private object GenerateToken(string login)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, login),
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

            var newRefreshToken = Guid.NewGuid();
            _dbService.AddRefreshToken(newRefreshToken, login);

            return new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = newRefreshToken
            };
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
