using Selenium.BaseComponents.Utilities;
using System;

namespace Selenium.BaseComponents.Data
{
    public static class Users
    {
        // Change the environment here
        public const string CurrentEnvironment = (PreBuildConstants.PREBUILD_ENV != "PREBUILD_ENV_VALUE") ? PreBuildConstants.PREBUILD_ENV : Environment.E2EP3;

        // List of environments alias
        public class Environment
        {
            public const string DEV01 = "DEV01";
            public const string INT01 = "INT01";

            public const string E2E = "E2E";
            public const string E2EP3 = "E2EP3";
            public const string DEV01P3 = "DEV01P3";
            public const string INT01P3 = "INT01P3";

            public const string PROD = "PROD";

        }
        // TestURL(string) - a hard-coded per-environment URL switch, deleted per
        // "getting rid of hardcoded test project values" (see AGENTS.md). Confirmed
        // via a full-solution grep it had zero call sites anywhere - dead code even
        // before this change. The real, live equivalent is
        // aut.Environment.EnvironmentUrl, configured via Environment Management and
        // resolved by BaseFeatureFixture.Url.



    }

}
