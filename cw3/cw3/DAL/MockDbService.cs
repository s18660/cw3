using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using cw3.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace cw3.DAL
{
    public class MockDbService : IDbService
    {
        public List<Student> _students { get; private set; }

        private string connectionString = "Data Source=db-mssql;Initial Catalog=s18660;Integrated Security=True";

        public MockDbService()
        {
            _students = new List<Student>();
        }

        public IEnumerable<Student> GetStudents()
        {
            using(var conn = new SqlConnection(connectionString))
            using(var command = new SqlCommand())
            {
                _students.Clear();
                command.Connection = conn;
                command.CommandText = "select * from Student";

                conn.Open();
                var dr = command.ExecuteReader();
                while(dr.Read())
                {
                    var stud = new Student();
                    stud.FirstName = dr["FirstName"].ToString();
                    stud.LastName = dr["LastName"].ToString();
                    stud.IndexNumber = dr["IndexNumber"].ToString();

                    _students.Add(stud);
                }
            }

            return _students;
        }

        public IEnumerable<Enrollment> GetStudentEnrollment(string id)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = conn;
                command.CommandText = "select Semester, IdStudy, StartDate from Enrollment where " +
                    "IdEnrollment = (select IdEnrollment from student where IndexNumber = @id)";
                command.Parameters.AddWithValue("id", id);

                conn.Open();
                var dr = command.ExecuteReader();
                var enrollments = new List<Enrollment>();
                while (dr.Read())
                {
                    var enrol = new Enrollment();
                    enrol.Semester = int.Parse(dr["Semester"].ToString());
                    enrol.IdStudy = int.Parse(dr["IdStudy"].ToString());
                    enrol.StartDate = DateTime.Parse(dr["StartDate"].ToString());

                    enrollments.Add(enrol);
                }
                return enrollments;
            }
        }

        public bool CheckCredentials(LoginRequestDto request)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select Salt from Student where IndexNumber = @index";
                command.Parameters.AddWithValue("index", request.Login);
                var dr = command.ExecuteReader();
                dr.Read();
                var salt = dr["Salt"].ToString();
                dr.Close();

                var hash = CreateHash(request.Password, salt);

                command.Connection = connection;
                command.CommandText = "select count(1) from Student where IndexNumber = @index and Password = @password";
                command.Parameters.AddWithValue("password", hash);
                dr = command.ExecuteReader();
                dr.Read();
                int count = (int)dr.GetValue(0);
                dr.Close();
                return count > 0 ? true : false;
            }
        }

        public string CheckRefreshToken(string refreshToken)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select indexnumber from Student where RefreshToken = @refreshToken";
                command.Parameters.AddWithValue("refreshToken", refreshToken);
                var dr = command.ExecuteReader();
                dr.Read();
                string login = dr["IndexNumber"].ToString();
                dr.Close();
                return login;
            }
        }

        public void AddRefreshToken(Guid refreshToken, string login)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "update student set RefreshToken = @refreshToken where IndexNumber = @login";
                command.Parameters.AddWithValue("refreshToken", refreshToken);
                command.Parameters.AddWithValue("login", login);
                var dr = command.ExecuteNonQuery();
            }
        }

        private string CreateHash(string password, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                                    password: password,
                                    salt: Encoding.UTF8.GetBytes(salt),
                                    prf: KeyDerivationPrf.HMACSHA512,
                                    iterationCount: 10000,
                                    numBytesRequested: 256 / 8);

            return Convert.ToBase64String(valueBytes);
        }

        private string CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}
