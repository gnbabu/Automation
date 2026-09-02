using PayrollTests.Models;

namespace PayrollTests.Services
{
    public class PayrollService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ITaxCalculator _taxCalculator;
        private readonly IBenefitsService _benefitsService;

        public PayrollService(
            IEmployeeRepository employeeRepo,
            ITaxCalculator taxCalculator,
            IBenefitsService benefitsService)
        {
            _employeeRepo = employeeRepo;
            _taxCalculator = taxCalculator;
            _benefitsService = benefitsService;
        }

        public PaySlip GeneratePaySlip(int employeeId)
        {
            var employee = _employeeRepo.GetEmployeeById(employeeId)
                ?? throw new ArgumentException("Employee not found");

            if (employee.BaseSalary < 0)
                throw new InvalidOperationException("Invalid salary amount");

            var tax = _taxCalculator.CalculateTax(employee.BaseSalary);
            if (tax < 0)
                throw new InvalidOperationException("Invalid tax amount");

            var benefits = _benefitsService.CalculateBenefits(employeeId);

            return new PaySlip
            {
                EmployeeId = employeeId,
                BaseSalary = employee.BaseSalary,
                TaxDeducted = tax,
                Benefits = benefits,
                NetPay = employee.BaseSalary - tax + benefits
            };
        }
    }
}
