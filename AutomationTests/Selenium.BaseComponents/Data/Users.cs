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
        public static string TestURL(string environment)
        {
            if (environment == "DEV")
            {
                return "https://ohpnm-dev.omes.maximus.com/OH_PNM_DEV/Account/Login.aspx";
            }
            if (environment == "INT01")
            {
                return "https://ohpnm-dev.omes.maximus.com/OH_PNM_INT01/Account/Login.aspx";
            }
            if (environment == "INT01P3")
            {
                return "https://ohpnm-dev.omes.maximus.com/OH_PNM_INT01P3/Account/Login.aspx";
            }
            if (environment == "DEV01P3")
            {
                return "https://ohpnm-dev.omes.maximus.com/OH_PNM_DEVP3/Account/Login.aspx";
            }
            if (environment == "E2EP3")
            {
                return "https://ohpnm-e2ep3.omes.maximus.com/OH_PNM_E2EP3/Account/Login.aspx";
            }
            return null;
        }





    }

}
