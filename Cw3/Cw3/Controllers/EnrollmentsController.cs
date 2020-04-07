using System;
using System.Data.SqlClient;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s18446;Integrated Security=True";

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            int idEnrollment;
            EnrollStudentResponse response;

            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                var tran = con.BeginTransaction();
                com.Transaction = tran;

                try
                {
                    // Czy studia istnieja?
                    com.CommandText = "select IdStudy from studies where name=@name";
                    com.Parameters.AddWithValue("name", request.studiesName);
                    Console.WriteLine("Tutaj jestem1");
                    var dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        tran.Rollback();
                        return BadRequest("Studia niedostepne");

                    }
                    int idStudies = (int)dr["IdStudy"];
                    dr.Close();

                    com.CommandText = "select idEnrollment from Enrollment where (StartDate = (select max(StartDate) from Enrollment)) and semester=1 and idStudy = @idStudy";
                    com.Parameters.AddWithValue("idStudy", idStudies);
                    Console.WriteLine("Tutaj jestem2");
                    dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        // dodac nowy enrollment
                          com.CommandText = "INSERT INTO Enrollment(idEnrollment, Semester, idStudy, StartDate) OUTPUT INSERTED.IDenrollment VALUES( " +
                                       "(select(select max(idEnrollment) from Enrollment) + 1), 1 , @idStudies,  (SELECT CONVERT(date, getdate())))";
                             com.Parameters.AddWithValue("idStudies", idStudies);
                             idEnrollment = com.ExecuteNonQuery();
                        _insertIntoEnrollments(com, idStudies);
                        Console.WriteLine("Tutaj jestem3");
                        com.CommandText = "select idEnrollment from Enrollment where (StartDate = (select max(StartDate) from Enrollment)) and semester=1 and idStudy = @idStudy2";
                        com.Parameters.AddWithValue("idStudy2", idStudies);
                        dr = com.ExecuteReader();
                        Console.WriteLine("Tutaj jestem4");
                         idEnrollment = (int)dr["IdEnrollment"];
                        Console.WriteLine("Tutaj jestem5");
                        dr.Close();
                    } else
                    {
                        idEnrollment = (int)dr["IdEnrollment"];
                        dr.Close();
                    }
                    Console.WriteLine("TUTAJ JESTEM 5");
                    
                    Console.WriteLine("Checking doubled indexes.");
                    com.CommandText = "select indexNumber from Student where indexNumber = @index";
                    com.Parameters.AddWithValue("index", request.IndexNumber);

                    dr = com.ExecuteReader();
                    if (dr.Read())
                    {
                        dr.Close();
                        tran.Rollback();
                        return BadRequest("Indeks nie jest unikalny.");
                    }
                    dr.Close();
                    tran.Commit();
                    //dodawanie studenta do bazy
                    Console.WriteLine("TUTAJ JESTEM 6");
                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment) ";
                    com.Parameters.AddWithValue("indexNumber", request.IndexNumber);
                    com.Parameters.AddWithValue("firstName", request.FirstName);
                    com.Parameters.AddWithValue("lastName", request.LastName);
                    com.Parameters.AddWithValue("birthDate", request.birthDate);
                    com.Parameters.AddWithValue("idEnrollment", idEnrollment);
                    com.ExecuteNonQuery();
                    Console.WriteLine("TUTAJ JESTEM 7");

                    //tworzenie obiektu enrollment do zwrocenia
                    //  com.CommandText = "select * from enrollment where idEnrollment = @idEnrollment";
                    //    com.Parameters.AddWithValue("idEnrollment", idEnrollment);
                    //    dr = com.ExecuteReader();

                    //   enrollment.idEnrollment = idEnrollment;
                    //    enrollment.idStudy = (int)dr["IdStudy"];
                    //   enrollment.semester = (int)dr["Semester"];
                    //    enrollment.startDate = (DateTime)dr["StartDate"];

                    response = new EnrollStudentResponse()
                    {
                        LastName = request.LastName,
                        Semester = 1,
                        StartDate = DateTime.Now
                    };
                  //  tran.Commit();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    tran.Rollback();
                    return BadRequest("Problem z bazka");       
                }

                return Created("Dodano studenta", response);
            }
        }


        private void _insertIntoEnrollments(SqlCommand command, int idStudy)
        {
            command.CommandText = "insert into Enrollment(IDENROLLMENT, SEMESTER, IDSTUDY, STARTDATE) "
                                  + "values ((select count(*)+1 from Enrollment), 1, @id_study, getdate())";
            command.Parameters.AddWithValue("id_study", idStudy);
            var reader = command.ExecuteReader();
            command.Parameters.Clear();
            reader.Close();
        }

        /*
        private readonly IDbService _dbService;
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s18446;Integrated Security=True";

        public EnrollmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {

           // var st = new Student();
            var enrollment = new Enrollment();
          //  st.FirstName = request.FirstName;
            
            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                var tran = con.BeginTransaction();
                try
                {
                    // Czy studia istnieja?
                    com.CommandText = "select IdStudies from studies where name=@name";
                    com.Parameters.AddWithValue("name", request.studiesName);

                    var dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        tran.Rollback();
                        return BadRequest("Studia niedostepne");
                        
                    }
                    int idStudies = (int)dr["IdStudies"];
                    dr.Close();
                    //znajdz studia i semestr w enrollments
                    Console.WriteLine("Checking if enrollment for studies exists.");
                    com.CommandText = "select idEnrollment from Enrollment where StartDate = (select max(StartDate) from Enrollment) and semester=1 and idStudy = @idStudy";   
                    com.Parameters.AddWithValue("idStudy", idStudies);
                    dr = com.ExecuteReader();
                    //   int idEnrollment;
                    int enrollmentID;
                    if (!dr.Read())
                    {
                        dr.Close();
                        //dodac nowy enrollment
                        com.CommandText = "INSERT INTO Enrollment(idEnrollment, Semester, idStudy, StartDate) VALUES( " +
                                    "((select max(idEnrollment) from Enrollment) + 1), 1 , @idStudies, SELECT CONVERT(date, getdate()))";
                        com.Parameters.AddWithValue("idStudies", idStudies);
                        com.ExecuteNonQuery();

                        com.CommandText = "select idEnrollment from Enrollment where idEnrollment = (select max(idEnrollment) from enrollment)";
                        dr = com.ExecuteReader();
                        enrollmentID = (int)dr["IdEnrollment"];
                        dr.Close();
                    }
                    else
                    {
                        enrollmentID = (int)dr["IdEnrollment"];
                        dr.Close();
                    }

                    Console.WriteLine("Checking doubled indexes.");
                    com.CommandText = "select index from Student where index=@index";
                    com.Parameters.AddWithValue("index", request.IndexNumber);

                    dr = com.ExecuteReader();
                    if(dr.Read())
                    {
                        dr.Close();
                        tran.Rollback();
                        return BadRequest("Indeks nie jest unikalny.");
                    }
                    dr.Close();

                    DateTime birthDate = DateTime.Parse(request.birthDate.Replace('.', '/'));
                    // Dodanie studenta
                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, idEnrollment)" +
                        " VALUES(@Index, @FirstName, @LastName, @BirthDate, @idEnrollment)";
                    com.Parameters.AddWithValue("Index", request.IndexNumber);
                    com.Parameters.AddWithValue("FirstName", request.FirstName);
                    com.Parameters.AddWithValue("LastName", request.LastName);
                    com.Parameters.AddWithValue("BirthDate", birthDate);
                    com.Parameters.AddWithValue("idEnrollment", enrollment.idEnrollment);
                    com.ExecuteNonQuery();

                    tran.Commit();
                }catch(SqlException ex)
                { 
                    tran.Rollback();
                    return BadRequest("Cos poszlo nie tak");
                }
            }
            
          //     var response = new EnrollStudentResponse();
          //  response.LastName = st.LastName;
            return Ok();
        } */
    }
}