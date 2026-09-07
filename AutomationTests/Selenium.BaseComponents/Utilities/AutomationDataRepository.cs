using System.Collections.Generic;
using System.Linq;

namespace Selenium.BaseComponents.Utilities
{
    // Consolidated from the near-identical per-project TC.*/Utilities/DataRepository.cs
    // copies (confirmed identical in structure - only the Model type and section-name
    // string differed, e.g. TC.PriorAuthSearch's "SearchPA" vs TC.SearchRA's
    // "SearchRAParams") - Phase 1 of the "consolidate duplicated boilerplate" rollout.
    // See AGENTS.md. Each project's own DataRepository.GetAutomationData(string) keeps
    // its existing public signature (callers already cast the returned object to their
    // own Model type, confirmed across TC.SearchRA/TC.SearchEligibility/TC.Registration/
    // TC.PriorAuthoriztion) and just delegates here with its specific Model type +
    // section name.
    //
    // Deliberately named AutomationDataRepository, not DataRepository - every TC.*
    // project already has its own "DataRepository" class in its own Utilities
    // namespace, and several of those test files already have both that namespace and
    // this one (Selenium.BaseComponents.Utilities) in scope via `using` - a same-named
    // class here would make every one of those projects' unqualified `DataRepository.
    // GetAutomationData(...)` calls ambiguous (confirmed by direct testing - a full
    // solution build failed with CS0104 in the 3 other already-migrated-callers
    // projects the moment this class was briefly named DataRepository), even in
    // projects this phase hasn't touched yet.
    public static class AutomationDataRepository
    {
        public static T GetAutomationData<T>(string flowName, string sectionName) where T : new()
        {
            var apiGateway = new APIGatway();
            List<AutomationData> automationDatas = apiGateway.GetAutomationData(flowName).Result;

            if (automationDatas == null || !automationDatas.Any())
                return default;

            var result = new T();

            foreach (var data in automationDatas)
            {
                if (data.SectionName == sectionName)
                    result = Mapper.BindData<T>(data.automationContents);
            }

            return result;
        }
    }
}

