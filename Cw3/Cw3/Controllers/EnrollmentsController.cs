using System;
using System.Data.SqlClient;
using System.Security.Claims;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cw3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService _dbService;
        public IConfiguration Configuration { get; set; }


        public EnrollmentsController(IStudentDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            Configuration = configuration;
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse response;
            try
            {
                response = _dbService.EnrollStudent(request);
            } catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Created("Dodano studenta", response);
        }
        

        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudent(PromoteStudentRequest promoteStudentRequest)
        {
            PromoteStudentResponse response;
            try
            {
                response = _dbService.PromoteStudents(promoteStudentRequest);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Created("Nadano promocje", response);
        }



        [HttpPost("login")]

        public IActionResult Login(LoginRequestStudent request)
        {
            LoginResponse response;
            try
            {
                response = _dbService.Login(Configuration, request);
                Console.WriteLine("co sie dzieje");
            }catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(response);      
        }
    }
 }