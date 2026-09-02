using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollTests.Models
{
    public interface IBenefitsService
    {
        decimal CalculateBenefits(int employeeId);
    }
}
