using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3.Data;

namespace cw3.Services
{
    public interface IStudentsDbService
    {
        public Enrollment AddStudent(Student student);
        public Enrollment Promotions(Promotion promotion);
        public bool CheckIndex(string index);
    }
}
