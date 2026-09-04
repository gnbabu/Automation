using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.PriorAuthSearch.Utilities
{
    public static class PageConstants
    {

    }

    public static class Roles
    {
        public static string StateAdmin => "StateAdmin";

        public static string EnrollementSpecialist => "EnrollementSpecialist";

        public static string ProviderAdmin => "ProviderAdmin";
    }

    public static class Tasks
    {
        public static string ProviderScreening = "Provider Screening";
        public static string ProviderReview = "Provider Review";
    }

    public static class Pages
    {
        public static string NPIAndMedID = "NPI and Med ID";
        public static string ApplicationDisposition = "Application Disposition";
    }
}
