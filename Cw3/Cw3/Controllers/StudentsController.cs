using Microsoft.AspNetCore.Mvc;
using Cw3.Models;
using Cw3.DAL;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using Cw3.DTOs.Requests;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Cw3.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        public IConfiguration Configuration { get; set; }
        private readonly IDbService _dbService;
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s18446;Integrated Security=True";

        public StudentsController(IDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            Configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "admin2")]
        public IActionResult GetStudents()
        {
            var list = new List<Student>();

            

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select IndexNumber ,firstName, LastName, BirthDate, Studies.Name, Enrollment.Semester from enrollment, student, studies " +
                                    "where Enrollment.IdEnrollment = student.IdEnrollment AND Enrollment.IdStudy = studies.IdStudy";

                con.Open();
                SqlDataReader reader = com.ExecuteReader();

                while(reader.Read())
                {
                    var student = new Student();
                    student.IndexNumber = reader["IndexNumber"].ToString();
                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.birthDate =DateTime.Parse(reader["BirthDate"].ToString()).Date;
                    student.studiesName = reader["Name"].ToString();
                    student.semester = reader["Semester"].ToString();

                    list.Add(student);
                }
                con.Dispose();
            }


          
            return Ok(list);
        }

        [HttpGet("{indexNumber}")]
        public IActionResult GetEnrollment(string indexNumber)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select Enrollment.IdEnrollment, Enrollment.IdStudy, Enrollment.Semester, Enrollment.StartDate FROM Enrollment, Student " +
                                    "where student.IndexNumber =@index AND Student.IdEnrollment = Enrollment.IdEnrollment";

                com.Parameters.AddWithValue("index", indexNumber);

                con.Open();
                        var reader = com.ExecuteReader();
                        if (reader.Read())
                        {
                            var enrollment = new Enrollment();
                            enrollment.idEnrollment = (int)reader["IdEnrollment"];
                            enrollment.idStudy = (int)reader["IdStudy"];
                            enrollment.semester = (int)reader["Semester"];
                            enrollment.startDate = (DateTime)reader["StartDate"];
                            return Ok(enrollment);
                        }
                con.Dispose();
            }
            return NotFound();
        }

        //SQL INJECTION asd':;DROP TABLE Student; --

        //public IActionResult GetStudent(string indexNumber)
        //{
        //    using (SqlConnection con = new SqlConnection(ConString))
        //    using (SqlCommand com = new SqlCommand())
        //    {
        //        com.Connection = con;
        //        com.CommandText = "SELECT * FROM STUDENT WHERE INDEXNUMBER='"+indexNumber+"'";

        //        con.Open();
        //        var reader = com.ExecuteReader();
        //        if (reader.Read())
        //        {
        //            var student = new Student();
        //            student.IndexNumber = reader["IndexNumber"].ToString();
        //            student.FirstName = reader["FirstName"].ToString();
        //            student.LastName = reader["LastName"].ToString();
        //            return Ok(student);
        //        }
        //    }
        //    return NotFound();
        //    if (id == 1)
        //{
        //    return Ok("Kowalski");
        //} else if (id == 2)
        //{
        //    return Ok("Malewski");
        //}

        //return NotFound("Nie znaleziono studenta");
        //   }

        [HttpPost]

        public IActionResult Login(LoginRequestDto request)
        {
            var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "loginjakis"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
                );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken=Guid.NewGuid()
            }
                );
        }
  /*      public IActionResult CreateStudent(Student student)
        {
            //... add to database
            //... generating index number
            student.IndexNumber = $"s{new System.Random().Next(1, 20000)}";
            return Ok(student);
        }
*/
        
        

        //[HttpPut("{id}")]
        //public IActionResult PutStudent(int id)
        //{
        //    return Ok($"{id} - Aktualizacja dokończona.");
        //}

        //[HttpDelete("{id}")]
        //public IActionResult DeleteStudent(int id)
        //{
        //    return Ok($"{id} - Usuwanie ukończone.");
        //}


    }
}