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
        public const string LoginUrl = "https://ohpnm-dev.omes.maximus.com/OH_PNM_INT01/Account/Login.aspx";

    }
}
