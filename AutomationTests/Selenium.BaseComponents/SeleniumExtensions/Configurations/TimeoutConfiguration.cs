using System;

namespace SeleniumExtensions.Configurations
{
    public class TimeoutConfiguration
    {
        public static TimeSpan Login => TimeSpan.FromSeconds(60);
        public static TimeSpan Redirect => TimeSpan.FromSeconds(60);
        public static TimeSpan Page => TimeSpan.FromSeconds(60);
        public static TimeSpan Element => TimeSpan.FromSeconds(60);
    }
}
