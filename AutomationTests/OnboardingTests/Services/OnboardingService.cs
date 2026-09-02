using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnboardingTests.Models;

namespace OnboardingTests.Services
{
    public class OnboardingService : IOnboardingService
    {
        public bool RegisterEmployee(Employee employee)
        {
            return employee.DocumentsVerified;
        }

        public Employee GetEmployeeById(int id)
        {
            return new Employee { Id = id, Name = "Test Employee", DocumentsVerified = true };
        }
    }
}
