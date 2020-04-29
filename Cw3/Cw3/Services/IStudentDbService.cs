using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.Services
{
    public interface IStudentDbService
    {
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        public PromoteStudentResponse PromoteStudents(PromoteStudentRequest request);
        public LoginResponse Login(IConfiguration configuration, LoginRequestStudent request);

        Student GetStudent(string index);
    }
}
