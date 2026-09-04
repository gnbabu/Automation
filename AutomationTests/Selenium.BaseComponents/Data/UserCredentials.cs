namespace Selenium.BaseComponents.Data
{
    public static class UserCredentials
    {
        public static class UserNameGenerator
        {
            //Change the username accordingly on your local repo if you want to use it in production 
            static Dictionary<string, string> usernames = new Dictionary<string, string>() {
                                                    {"StateAdmin","aktech"},
                                                    {"ProviderAdmin","fjprovadmin"},
                                                    {"EnrollementSpecialist","enrollmentAutomation" },
                                                    {"TechAdmin","autotechadmin" }
                                                   };
            public static string GetUserName(string Profile, string environment)
            {
                string userName;
                if (environment == null || environment == "prod" || environment == "so")
                {
                    userName = usernames[Profile];
                }
                else
                {
                    userName = usernames[Profile];// + "." + environment;
                }
                return userName;
            }
        }

        public static class PasswordGenerator
        {
            static Dictionary<string, string> passwords = new Dictionary<string, string>() {
                                                {"DEV01","Abcde12!"},
                                                {"DEV01P3","Abcde12!"},
                                                {"INT01","Abcde12!"},
                                                {"E2EP3","Abcde123!"},
                                                {"INT01P3","Abcde12!"},
                                                {"E2E01P3","Abcde12!"},
                                                {"PROD","Abcde12!"},
                                            };
            public static string GetPassword(string environment)
            {
                return passwords[environment];
            }
        }
    }
}
