using System;
using System.Collections.Generic;

namespace TestDataModels
{
   
    public class SubmitPAProfessional    // FlowName
    {
        public ProviderHeaderInfomration ProviderHeaderInfomration { get; set; } = new()   // section
        {
            ProviderMedicaidId = "2422659",
            ProviderNpi = "1285673111",
            ProviderName = "CROSSROADS HOSPICE OF CINCINNATI, LLC",
            PriorAuthorizationType = "Professional",
            DestinationPayer = "Ohio Department of Medicaid",
            Assignment = "Home Health",
            ServiceType = "Alcoholism",
        };
        // Recipient Information
        public RecipientInfo Recipient { get; set; } = new()
        {
            MedicaidBillingNumber = "102420808299",
            DateOfBirth = new DateTime(1962, 7, 9),
        };

        // Requester Information
        public RequesterContactInfo Requester { get; set; } = new()
        {
            FirstName = "Danyele",
            LastName = "Loughridge",
            ContactNumber = "6145059558",
        };

        // Service Information
        public ServiceInformation ServiceInformation { get; set; } = new()
        {
            PlaceOfService = "12",
            LevelOfService = "E - Elective",
        };

        // Provider Information
        public ServiceProviderInfo ServiceProvider { get; set; } = new()
        {
            Npi = "1114481405",
            MedicaidId = "0405165",
            OrganizationName = "HOPEHEALTH LTD.",
        };

        public OrderingProviderInfo OrderingProvider { get; set; } = new()
        {
            Npi = "1629272687",
        };

        // Diagnosis Information
        public List<DiagnosisInfo> Diagnoses { get; set; } = new()
        {
            new DiagnosisInfo
            {
                Sequence = 1,
                DiagnosisCode = "E1122",
                DiagnosisDate = new DateTime(2026, 10, 1),
            },
            new DiagnosisInfo
            {
                Sequence = 2,
                DiagnosisCodeType = "Admitting",
                DiagnosisCode = "I5022",
                DiagnosisDate = new DateTime(2026, 10, 1),
            },
        };

        // Service Details
        public List<ServiceDetail> ServiceDetails { get; set; } = new()
        {
            new ServiceDetail
            {
                LineNumber = 1,
                ProcedureCode = "G0299",
                Modifier1 = "12",
                Modifier2 = "12",
                Modifier3 = "12",
                Modifier4 = "12",
                RequestedUnits = 40,
                RequestedDollars = 370m,
                RequestedFromDate = new DateTime(2026, 10, 1),
                RequestedToDate = new DateTime(2026, 10, 20),
            },
            new ServiceDetail
            {
                LineNumber = 2,
                ProcedureCode = "G0151",
                Modifier1 = "13",
                Modifier2 = "13",
                Modifier3 = "13",
                Modifier4 = "13",
                RequestedUnits = 32,
                RequestedDollars = 760m,
                RequestedFromDate = new DateTime(2026, 10, 8),
                RequestedToDate = new DateTime(2026, 10, 29),
            },
        };

        // Provider Notes
        public string ProviderNotes { get; set; }

        // Reviewer Notes
        public List<ReviewerNote> ReviewerNotes { get; set; } = new();

        // Outcome Of Review
        public List<OutcomeOfReview> Outcomes { get; set; } = new();

        // Attachments
        public List<AttachmentInfo> Attachments { get; set; } = new()
        {
            new AttachmentInfo
            {
                LineItem = 1,
                DocumentType = "X-Rays",
                Note = "test",
                FilePath = @"TestFiles\SampleAttachment.pdf",
            },
        };
    }

    public class ProviderHeaderInfomration
    {
        // Header Information
        public string ProviderMedicaidId { get; set; }
        public string ProviderNpi { get; set; }
        public string ProviderName { get; set; }

        public string PriorAuthorizationType { get; set; }
        public string DestinationPayer { get; set; }
        public string Assignment { get; set; }
        public string DestinationPayerId { get; set; }
        public string ServiceType { get; set; }

        // PA Information
        public string PaStatus { get; set; }
        public string PaNumber { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class RecipientInfo
    {
        public string MedicaidBillingNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PatientTrackingNumber { get; set; }
        public string Gender { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    public class RequesterContactInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNumber { get; set; }
        public string Extension { get; set; }
    }

    public class ServiceInformation
    {
        public string PlaceOfService { get; set; }
        public string LevelOfService { get; set; }
        public string DelayReason { get; set; }

        public DateTime? AccidentDate { get; set; }
        public DateTime? DateOfPatientEvent { get; set; }
        public DateTime? DateOfOnsetOfIllness { get; set; }
        public DateTime? LastMenstrualPeriodDate { get; set; }
        public DateTime? EstimatedBirthDate { get; set; }

        public string AssociatePaNumber { get; set; }
    }

    public class ServiceProviderInfo
    {
        public string Npi { get; set; }
        public string MedicaidId { get; set; }
        public string OrganizationName { get; set; }
        public string FirstName { get; set; }
    }

    public class OrderingProviderInfo
    {
        public string Npi { get; set; }
        public string MedicaidId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    public class DiagnosisInfo
    {
        public int Sequence { get; set; }
        public string DiagnosisCodeType { get; set; }
        public string DiagnosisCode { get; set; }
        public string DiagnosisDescription { get; set; }
        public DateTime? DiagnosisDate { get; set; }
    }

    public class ServiceDetail
    {
        public int LineNumber { get; set; }
        public string ProcedureCode { get; set; }
        public string Modifier1 { get; set; }
        public string Modifier2 { get; set; }
        public string Modifier3 { get; set; }
        public string Modifier4 { get; set; }
        public int RequestedUnits { get; set; }
        public decimal RequestedDollars { get; set; }
        public DateTime? RequestedFromDate { get; set; }
        public DateTime? RequestedToDate { get; set; }
        public string Status { get; set; }
    }

    public class ReviewerNote
    {
        public int LineNumber { get; set; }
        public string Note { get; set; }
    }

    public class OutcomeOfReview
    {
        public int LineNumber { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonDescription { get; set; }
    }

    public class AttachmentInfo
    {
        public int LineItem { get; set; }

        public string DocumentId { get; set; }

        public string PatientTrackingNumber { get; set; }

        public string DocumentType { get; set; }

        public string Note { get; set; }

        public string FilePath { get; set; }
    }
}
