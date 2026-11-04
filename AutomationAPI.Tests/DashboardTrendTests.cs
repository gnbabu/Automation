using AutomationAPI.Controllers;
using NUnit.Framework;

namespace AutomationAPI.Tests
{
    [TestFixture]
    public class DashboardTrendTests
    {
        [TestCase(0, 7)]
        [TestCase(3, 7)]
        [TestCase(7, 7)]
        [TestCase(14, 14)]
        [TestCase(30, 30)]
        [TestCase(90, 30)]
        public void TrendDays_AreClampedTo7Through30(int requested, int expected)
        {
            Assert.That(DashboardController.ClampTrendDays(requested), Is.EqualTo(expected));
        }

        [Test]
        public void Testers_SeeOnlyTheirOwnTestCases()
        {
            Assert.That(DashboardController.TrendScopeUser(isTester: true, userId: 5), Is.EqualTo(5));
        }

        [Test]
        public void OtherRoles_SeeTheWholeRelease()
        {
            Assert.That(DashboardController.TrendScopeUser(isTester: false, userId: 5), Is.Null);
        }
    }
}
