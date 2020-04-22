using cw3.Data;
using cw3.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace cw3.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private readonly string connectionString = "Data Source=db-mssql;Initial Catalog=s18660;Integrated Security=True;MultipleActiveResultsets=true";

        public Enrollment AddStudent(Student student)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                
                var tran = connection.BeginTransaction();
                command.Transaction = tran;

                try
                {
                    command.CommandText = "select IdStudy from studies where name=@name";
                    command.Parameters.AddWithValue("name", student.Studies);

                    var dr = command.ExecuteReader();
                    if (!dr.Read())
                    {
                        throw new Exception("Brak podanych studiów");
                    }
                    int idStudy = (int)dr["IdStudy"];
                    dr.Close();

                    command.CommandText = "select IdEnrollment from Enrollment where idstudy = @idstudy and semester = 1";
                    command.Parameters.AddWithValue("idstudy", idStudy);
                    dr = command.ExecuteReader();

                    if(!dr.Read())
                    {
                        dr.Close();
                        command.CommandText = "Insert Into Enrollment(IdEnrollment, Semester, IdStudy, StartDate)" +
                                              "Select max(IdEnrollment) + 1, 1, @idstudy, getdate() from Enrollment";
                        command.ExecuteNonQuery();

                        command.CommandText = "select IdEnrollment from Enrollment where idstudy = @idstudy and semester = 1";
                        dr = command.ExecuteReader();
                        dr.Read();
                    }
                    int idEnrollment = (int)dr["IdEnrollment"];
                    dr.Close();

                    command.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)" +
                                            "VALUES(@index, @fname, @lname, @bdate, @idEnrollment)";
                    command.Parameters.AddWithValue("index", student.IndexNumber);
                    command.Parameters.AddWithValue("fname", student.FirstName);
                    command.Parameters.AddWithValue("lname", student.LastName);
                    command.Parameters.AddWithValue("bdate", student.BirthDate);
                    command.Parameters.AddWithValue("idEnrollment", idEnrollment);
                    command.ExecuteNonQuery();

                    tran.Commit();
                    tran.Dispose();
                    return new Enrollment() {IdStudy = idStudy, Semester = 1, StartDate = DateTime.Today };
                }
                catch (SqlException e)
                {
                    tran.Rollback();
                    tran.Dispose();
                    throw new Exception(e.Message);
                }
            }
        }

        public bool CheckIndex(string index)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select count(1) from Student where IndexNumber = @index";
                command.Parameters.AddWithValue("index", index);
                var dr = command.ExecuteReader();
                dr.Read();
                int count = (int)dr.GetValue(0);
                dr.Close();
                return count > 0 ? true : false;
            }
        }

        public Enrollment Promotions(Promotion promotion)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                try
                {
                    command.CommandText = "select  count(1) from Enrollment e join Studies s on e.idstudy = s.idstudy " +
                                            "where s.name = @name and e.Semester = @semester";
                    command.Parameters.AddWithValue("name", promotion.Studies);
                    command.Parameters.AddWithValue("semester", promotion.Semester);
                    var dr = command.ExecuteReader();
                    dr.Read();
                    int count = (int)dr.GetValue(0);
                    dr.Close();
                    if (count < 1)
                    {
                        throw new Exception("Brak podanych studiów");
                    }

                    command.CommandText = "exec promoteStudents @name, @semester";
                    command.ExecuteNonQuery();

                    command.CommandText = "select s.IdStudy, StartDate from Enrollment e join Studies s on e.idstudy = s.idstudy " +
                                            "where s.name = @name and e.Semester = @semester + 1";
                    dr = command.ExecuteReader();
                    dr.Read();
                    int idStudy = (int)dr["IdStudy"];
                    DateTime startDate = DateTime.Parse(dr["startDate"].ToString());

                    return new Enrollment() {IdStudy = idStudy, Semester = promotion.Semester + 1, StartDate = startDate};
                }
                catch (SqlException e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public IEnumerable<Student> GetStudents()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = conn;
                command.CommandText = "select * from Student";

                conn.Open();
                var dr = command.ExecuteReader();
                var students = new List<Student>();
                while (dr.Read())
                {
                    var stud = new Student();
                    stud.FirstName = dr["FirstName"].ToString();
                    stud.LastName = dr["LastName"].ToString();
                    stud.IndexNumber = dr["IndexNumber"].ToString();

                    students.Add(stud);
                }
                return students;
            }
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
                string login = "";

                if (dr.HasRows)
                {
                    login = dr["IndexNumber"].ToString();
                }

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
