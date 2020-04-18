using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project01.Models;

namespace Project01.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {

        private readonly string connectionString = @"Data Source=(LocalDb)\LocalDatabase;Initial Catalog=semester4_dotnet;Integrated Security=True";

        [HttpGet]
        public IActionResult GetStudent()
        {

            List<Student> studentsList = new List<Student>();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select s.IndexNumber, s.Firstname, s.Lastname, s.BirthDate, st.Name as Studies, e.Semester
                                           from Student s
                                           join Enrollment e on e.IdEnrollment = s.IdEnrollment 
                                           join Studies st on st.IdStudy = e.IdStudy";
                    connection.Open();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {

                        var student = new Student
                        {
                            IndexNumber = reader["IndexNumber"].ToString(),
                            FirstName = reader["Firstname"].ToString(),
                            LastName = reader["Lastname"].ToString(),
                            BirthDate = DateTime.Parse(reader["BirthDate"].ToString()),
                            Studies = reader["Studies"].ToString(),
                            Semester = int.Parse(reader["Semester"].ToString())
                        };
                        studentsList.Add(student);
                    }
                }
            }

            return Ok(studentsList);
        }

        // *********************************************************************************************************************************************

        [HttpGet("getsemester/{id}")]
        public IActionResult GetSemesterEntry(string id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @$"select e.Semester
                                            from Enrollment e 
                                            join Student s on s.IdEnrollment = e.IdEnrollment 
                                            where s.IndexNumber=@id";
                    command.Parameters.AddWithValue("id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read() == false)
                    {
                        return NotFound($"Student with id {id} does not exist in the list!");
                    }
                    else
                    {
                        int semester = Convert.ToInt32(reader["Semester"]);
                        return Ok($"{id} -- {semester}");
                    }
                }
            }
        }
        // *******************************************************************************************************************************
        // For task 3.4, I tried to pass "Jason' OR '1'='1" as an id for the get method GetSemesterEntry(string id).
        // And it returned one semester entry record. After that I assigned id variable to the query using it as a parameter (@id). 
        // After that, I checked the method again using the Postman. 
        // This time SQL Injection did not happen and the method returned an error

        // *********************************************************************************************************************************************

        // GET api/students/getdobusingname/bob
        [HttpGet("getdobusingname/{name}")]
        public IActionResult GetDOBusingName(string name)
        {
            var birthDates = new List<string>();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = $"SELECT BirthDate FROM Students WHERE FirstName=@name;";
                    command.Parameters.AddWithValue("name", name);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string date = reader["BirthDate"].ToString();
                        birthDates.Add(date);
                    }
                    if (birthDates.Count == 0)
                        return NotFound($"This name is not registered to the system!");
                }
            }

            return Ok(birthDates);
        }
    }
}
