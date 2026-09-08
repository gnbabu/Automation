namespace Selenium.BaseComponents.Utilities
{
    public enum ElementPropertyFilter
    {
        equals,
        notequals,
        contains,
        startswith,
        endswith
    }
    public class BaseConstants
    {
        public const string Prod = "prod";
    }
    public class WebOptions
    {
        public const string HeadLess = "--headless";
        public const string AGENT_MACHINENAME = "AGENT_MACHINENAME";
        public const string Disable_notifications = "--disable-notifications";
        // LoginUrl (hard-coded INT01 login page) removed - confirmed via a full-solution
        // grep it had zero call sites anywhere, same as the other dead hard-coded values
        // already removed (Users.TestURL, UserCredentials.cs). The real, live equivalent
        // is aut.Environment.EnvironmentUrl, resolved by BaseFeatureFixture.Url.
    }
}
