using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnboardingTests.Models;

namespace OnboardingTests.Services
{
    public interface IOnboardingService
    {
        bool RegisterEmployee(Employee employee);
        Employee GetEmployeeById(int id);
    }
}
