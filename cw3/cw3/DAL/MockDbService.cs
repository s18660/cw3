using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.DAL
{
    public class MockDbService : IDbService
    {
        public static IEnumerable<Student> _students { get; private set; }

        static MockDbService()
        {
            _students = new List<Student>
            {
                new Student{IdStudent=1, FistName="Norbert", LastName="Gierczak"},
                new Student{IdStudent=2, FistName="Witold", LastName="Gutek"},
                new Student{IdStudent=3, FistName="Paweł", LastName="Szabla"}
            };
        }

        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
