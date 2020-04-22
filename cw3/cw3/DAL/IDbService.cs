using cw3.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
        public IEnumerable<Enrollment> GetStudentEnrollment(string id);
        public bool CheckCredentials(LoginRequestDto request);
        public string CheckRefreshToken(string refreshToken);
        public void AddRefreshToken(Guid refreshToken, string login);
    }
}
