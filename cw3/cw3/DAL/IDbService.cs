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
    }
}
