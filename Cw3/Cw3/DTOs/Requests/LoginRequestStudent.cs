using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.DTOs.Requests
{
    public class LoginRequestStudent
    {
        [Required]
        public string IndexNumber { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
