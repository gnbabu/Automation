using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollTests.Models
{
    public interface IEmployeeRepository
    {
        Employee GetEmployeeById(int employeeId);
    }
}
