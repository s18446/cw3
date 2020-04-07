using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.Models
{
    public class Enrollment
    {
        public int idEnrollment { get; set; }

        public int semester { get; set; }

        public int idStudy { get; set; }

        public DateTime startDate { get; set; }
    }
}
