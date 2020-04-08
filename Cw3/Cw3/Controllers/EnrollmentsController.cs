using System;
using System.Data.SqlClient;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService _dbService;

        public EnrollmentsController(IStudentDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
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
    }
 }