using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollTests.Models
{
    public interface ITaxCalculator
    {
        decimal CalculateTax(decimal baseSalary);
    }
}
