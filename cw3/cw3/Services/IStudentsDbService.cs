using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3.Data;
using cw3.DTOs;

namespace cw3.Services
{
    public interface IStudentsDbService
    {
        public IEnumerable<Student> GetStudents();
        public IEnumerable<Enrollment> GetStudentEnrollment(string id);
        public Enrollment AddStudent(Student student);
        public Enrollment Promotions(Promotion promotion);
        public bool CheckIndex(string index);
        public bool CheckCredentials(LoginRequestDto request);
        public string CheckRefreshToken(string refreshToken);
        public void AddRefreshToken(Guid refreshToken, string login);
    }
}
