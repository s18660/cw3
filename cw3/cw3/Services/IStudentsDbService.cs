using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3.Data;

namespace cw3.Services
{
    public interface IStudentsDbService
    {
        public Enrollment addStudent(Student student);
        public Enrollment promotions(Promotion promotion);
    }
}
