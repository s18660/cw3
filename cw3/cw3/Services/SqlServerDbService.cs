using cw3.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private readonly string connectionString = "Data Source=db-mssql;Initial Catalog=s18660;Integrated Security=True;MultipleActiveResultsets=true";

        public Enrollment addStudent(Student student)
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
                        return null;
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
                    return new Enrollment() {IdStudy = idStudy, Semester = 1, StartDate = DateTime.Today };
                }
                catch (SqlException e)
                {
                    tran.Rollback();
                    return null;
                }
            }
        }

        public Enrollment promotions(Promotion promotion)
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
                        return null;
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
                    return null;
                }
            }
        }
    }
}
