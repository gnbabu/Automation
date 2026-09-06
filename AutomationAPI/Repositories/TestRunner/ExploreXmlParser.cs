using System.Xml;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.TestRunner
{
    // Converts NUnit.Engine's Explore() XML into this app's existing ClassInfo/
    // LibraryMethodInfo shape, so TestSuitesRepository's public contract (and therefore
    // the frontend) doesn't need to change at all for this refactor. Correctly includes
    // every concrete test case NUnit itself would run - [TestCase]/[TestCaseSource]/
    // [Values]/[Range]/[Combinatorial]/[TestFixtureSource]-expanded cases all show up as
    // their own <test-case>, unlike the old reflection scan which only recognized the
    // method-level [Test]/[TestCase]/[TestCaseSource] attributes and never expanded them.
    public static class ExploreXmlParser
    {
        public static List<ClassInfo> ParseClasses(XmlNode exploreResult)
        {
            var classes = new List<ClassInfo>();

            // type="TestFixture" test-suites are exactly the [TestFixture]-decorated
            // classes - "name" is the simple (non-namespace-qualified) class name,
            // matching the old reflection scan's `t.Name` exactly.
            //
            // Fixed: for a *parameterized* fixture (e.g. [TestFixture("TechAdmin")]),
            // NUnit's "name" attribute is the display form including the constructor arg
            // (e.g. `SearchPATest("TechAdmin")`) - confirmed by direct testing this got
            // stored as ClassName and round-tripped all the way to a real Test Case
            // Assignment, where NUnitEngineHelper.FindMatchingTestCases (used to build the
            // filter for Run()) couldn't match it against anything, since NUnit's own
            // <test-case> classname attribute never includes the constructor-arg text -
            // the assigned test became silently unrunnable ("No test matching Class=...").
            // The fixture node's own "classname" attribute doesn't have this problem (it's
            // always the plain fully-qualified class name); use that, reduced to the
            // simple name the same way "name" already was for the non-parameterized case.
            var fixtureNodes = exploreResult.SelectNodes("//test-suite[@type='TestFixture']");
            if (fixtureNodes == null) return classes;

            foreach (XmlNode fixtureNode in fixtureNodes)
            {
                var fullClassName = fixtureNode.Attributes?["classname"]?.Value;
                var className = !string.IsNullOrEmpty(fullClassName)
                    ? fullClassName.Substring(fullClassName.LastIndexOf('.') + 1)
                    : (fixtureNode.Attributes?["name"]?.Value ?? "");

                var methods = new List<LibraryMethodInfo>();
                // ".//test-case" (descendant), not just direct children - a method with
                // multiple [TestCase] rows, [TestCaseSource], or [Values]/[Range]/
                // [Combinatorial] parameters is nested by NUnit under an intermediate
                // <test-suite type="ParameterizedMethod"> wrapper, one level below the
                // fixture, with one <test-case> per generated data row.
                var testCaseNodes = fixtureNode.SelectNodes(".//test-case");
                if (testCaseNodes != null)
                {
                    foreach (XmlNode testCaseNode in testCaseNodes)
                    {
                        methods.Add(new LibraryMethodInfo
                        {
                            MethodName = testCaseNode.Attributes?["methodname"]?.Value ?? testCaseNode.Attributes?["name"]?.Value ?? "",
                            Description = GetProperty(testCaseNode, "Description"),
                            Priority = GetProperty(testCaseNode, "Priority"),
                            TestCaseId = GetProperty(testCaseNode, "TestCaseId")
                        });
                    }
                }

                if (methods.Any())
                {
                    classes.Add(new ClassInfo
                    {
                        ClassName = className,
                        Methods = methods
                    });
                }
            }

            return classes;
        }

        // A method-level [Property(...)] attribute (Description/Priority/TestCaseId) is
        // attached by NUnit to the intermediate <test-suite type="ParameterizedMethod">
        // wrapper node when a method has multiple generated test cases (multiple [TestCase]
        // rows, [TestCaseSource], [Values]/[Range]/[Combinatorial]), NOT to each individual
        // <test-case> child - confirmed by direct testing (Explore() on a method with 3
        // [TestCase] rows put Description/Priority/TestCaseId only on the parent
        // ParameterizedMethod node, leaving each <test-case> with no <properties> of its
        // own at all). Walk up through ancestor <test-suite> nodes (stopping once we leave
        // the fixture, i.e. hit a node without a "classname" attribute) so a plain [Test]
        // method (property directly on its own <test-case>) and a parameterized one (
        // property one level up) both resolve correctly - all data rows of one method
        // legitimately share the same method-level property values.
        private static string GetProperty(XmlNode testCaseNode, string propertyName)
        {
            for (var node = testCaseNode; node != null && node.Attributes?["classname"] != null; node = node.ParentNode)
            {
                var propNode = node.SelectSingleNode($"properties/property[@name='{propertyName}']");
                if (propNode != null)
                    return propNode.Attributes?["value"]?.Value ?? "";
            }

            return "";
        }
    }
}
