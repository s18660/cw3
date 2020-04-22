using cw3.Data;
using cw3.DTOs;
using cw3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace cw3.Controllers
{
    [ApiController]
    [Authorize(Roles = "employee")]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        private readonly IConfiguration Configuration;

        public EnrollmentsController(IConfiguration configuration, IStudentsDbService dbService)
        {
            Configuration = configuration;
            _dbService = dbService;
        }

        [HttpPost("addStudent")]
        public IActionResult AddStudent(Student student)
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
        public IActionResult Promotions(Promotion promotion)
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

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult LoginEmployee(LoginRequestDto request)
        {
            if (!_dbService.CheckCredentials(request))
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Login),
                new Claim(ClaimTypes.Role, "employee")
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
                new Claim(ClaimTypes.Role, "employee")
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
    }
}