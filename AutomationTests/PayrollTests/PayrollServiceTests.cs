using Moq;
using PayrollTests.Models;
using PayrollTests.Services;

[TestFixture]
public class PayrollServiceTests
{
    private Mock<IEmployeeRepository> _employeeRepoMock;
    private Mock<ITaxCalculator> _taxCalculatorMock;
    private Mock<IBenefitsService> _benefitsServiceMock;
    private PayrollService _payrollService;

    [SetUp]
    public void Setup()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _taxCalculatorMock = new Mock<ITaxCalculator>();
        _benefitsServiceMock = new Mock<IBenefitsService>();

        _payrollService = new PayrollService(
            _employeeRepoMock.Object,
            _taxCalculatorMock.Object,
            _benefitsServiceMock.Object
        );
    }

    // ==================================================
    // TC_PYSLIP_001
    // ==================================================
    [Test]
    [Property("Description", "Valid employee should generate a payslip with correct tax, benefits, and net pay.")]
    [Property("Priority", "High")]
    [Property("TestCaseId", "TC_PYSLIP_001")]
    public void GeneratePaySlip_ValidEmployee_ReturnsCorrectNetPay()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(1))
            .Returns(new Employee { Id = 1, BaseSalary = 5000 });

        _taxCalculatorMock.Setup(x => x.CalculateTax(5000)).Returns(500);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(1)).Returns(200);

        var payslip = _payrollService.GeneratePaySlip(1);

        Assert.AreEqual(1, payslip.EmployeeId);
        Assert.AreEqual(5000, payslip.BaseSalary);
        Assert.AreEqual(500, payslip.TaxDeducted);
        Assert.AreEqual(200, payslip.Benefits);
        Assert.AreEqual(4700, payslip.NetPay);
    }

    // ==================================================
    // TC_PYSLIP_002
    // ==================================================
    [Test]
    [Property("Description", "When salary is zero, the net pay must be zero.")]
    [Property("Priority", "Medium")]
    [Property("TestCaseId", "TC_PYSLIP_002")]
    public void GeneratePaySlip_ZeroSalary_ZeroNetPay()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(5)).Returns(new Employee { Id = 5, BaseSalary = 0 });
        _taxCalculatorMock.Setup(x => x.CalculateTax(0)).Returns(0);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(5)).Returns(0);

        var payslip = _payrollService.GeneratePaySlip(5);

        Assert.AreEqual(0, payslip.NetPay);
    }

    // ==================================================
    // TC_PYSLIP_003
    // ==================================================
    [Test]
    [Property("Description", "Negative benefits should reduce the net pay correctly.")]
    [Property("Priority", "Medium")]
    [Property("TestCaseId", "TC_PYSLIP_003")]
    public void GeneratePaySlip_NegativeBenefits_CorrectNetPay()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(4)).Returns(new Employee { Id = 4, BaseSalary = 4000 });
        _taxCalculatorMock.Setup(x => x.CalculateTax(4000)).Returns(400);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(4)).Returns(-50);

        var payslip = _payrollService.GeneratePaySlip(4);

        Assert.AreEqual(3550, payslip.NetPay);
    }

    // ==================================================
    // TC_PYSLIP_004
    // ==================================================
    [Test]
    [Property("Description", "When tax is zero, only benefits should increase the net pay.")]
    [Property("Priority", "Medium")]
    [Property("TestCaseId", "TC_PYSLIP_004")]
    public void GeneratePaySlip_TaxZero_BenefitsOnlyAdded()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(2)).Returns(new Employee { Id = 2, BaseSalary = 3000 });
        _taxCalculatorMock.Setup(x => x.CalculateTax(3000)).Returns(0);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(2)).Returns(100);

        var payslip = _payrollService.GeneratePaySlip(2);

        Assert.AreEqual(3100, payslip.NetPay);
    }

    // ==================================================
    // TC_PYSLIP_005
    // ==================================================
    [Test]
    [Property("Description", "High salary should calculate correct tax, benefits, and net pay.")]
    [Property("Priority", "High")]
    [Property("TestCaseId", "TC_PYSLIP_005")]
    public void GeneratePaySlip_HighSalary_CorrectlyCalculated()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(8)).Returns(new Employee { Id = 8, BaseSalary = 100000 });
        _taxCalculatorMock.Setup(x => x.CalculateTax(100000)).Returns(25000);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(8)).Returns(5000);

        var payslip = _payrollService.GeneratePaySlip(8);

        Assert.AreEqual(80000, payslip.NetPay);
    }

    // ==================================================
    // TC_PYSLIP_006
    // ==================================================
    [Test]
    [Property("Description", "GeneratePaySlip must rethrow exceptions from BenefitsService.")]
    [Property("Priority", "Low")]
    [Property("TestCaseId", "TC_PYSLIP_006")]
    public void GeneratePaySlip_BenefitsServiceThrows_ThrowsException()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(9)).Returns(new Employee { Id = 9, BaseSalary = 7000 });
        _taxCalculatorMock.Setup(x => x.CalculateTax(7000)).Returns(700);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(9)).Throws(new Exception("Service error"));

        var ex = Assert.Throws<Exception>(() => _payrollService.GeneratePaySlip(9));
        Assert.That(ex.Message, Is.EqualTo("Service error"));
    }

    // ==================================================
    // TC_PYSLIP_007
    // ==================================================
    [Test]
    [Property("Description", "Ensure all dependencies are called exactly once for valid payslip generation.")]
    [Property("Priority", "High")]
    [Property("TestCaseId", "TC_PYSLIP_007")]
    public void GeneratePaySlip_VerifiesDependencyCalls()
    {
        _employeeRepoMock.Setup(x => x.GetEmployeeById(10)).Returns(new Employee { Id = 10, BaseSalary = 6000 });
        _taxCalculatorMock.Setup(x => x.CalculateTax(6000)).Returns(600);
        _benefitsServiceMock.Setup(x => x.CalculateBenefits(10)).Returns(300);

        var payslip = _payrollService.GeneratePaySlip(10);

        _employeeRepoMock.Verify(x => x.GetEmployeeById(10), Times.Once);
        _taxCalculatorMock.Verify(x => x.CalculateTax(6000), Times.Once);
        _benefitsServiceMock.Verify(x => x.CalculateBenefits(10), Times.Once);
    }
}
