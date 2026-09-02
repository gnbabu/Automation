using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollTests.Models
{
    public class PaySlip
    {
        public int EmployeeId { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TaxDeducted { get; set; }
        public decimal Benefits { get; set; }
        public decimal NetPay { get; set; }
    }
}
