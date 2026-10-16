using System;
using System.Collections.Generic;

namespace TestDataModels
{
   
    // Property initializers below are the flow's default values: the Portal shows them
    // for any section a test case hasn't saved yet, and test runs use them for those
    // sections. Values match the working Dental PA data in E2EP3 where they apply.
    public class SubmitPAProfessional    // FlowName
    {
        public ProviderHeaderInfomration ProviderHeaderInfomration { get; set; } = new()   // section
        {
            ProviderMedicaidId = "00059871",
            ProviderNpi = "1932384654",
            PriorAuthorizationType = "Professional",
            DestinationPayer = "Ohio Department of Medicaid",
            Assignment = "Professional",
        };
        // Recipient Information
        public RecipientInfo Recipient { get; set; } = new()
        {
            MedicaidBillingNumber = "0005987",
            DateOfBirth = new DateTime(1985, 6, 12),
            PatientTrackingNumber = "PTN778899",
        };

        // Requester Information
        public RequesterContactInfo Requester { get; set; } = new()
        {
            FirstName = "Naresh",
            LastName = "Gupta",
            ContactNumber = "6145551234",
            Extension = "101",
        };

        // Service Information
        public ServiceInformation ServiceInformation { get; set; } = new()
        {
            PlaceOfService = "11",
            LevelOfService = "E - Elective",
        };

        // Provider Information
        public ServiceProviderInfo ServiceProvider { get; set; } = new()
        {
            Npi = "1932384654",
        };

        public OrderingProviderInfo OrderingProvider { get; set; } = new()
        {
            Npi = "1932384654",
        };

        // Diagnosis Information
        public List<DiagnosisInfo> Diagnoses { get; set; } = new()
        {
            new DiagnosisInfo
            {
                Sequence = 1,
                DiagnosisCodeType = "ICD-10",
                DiagnosisCode = "Z00.00",
                DiagnosisDescription = "Encounter for general adult medical examination without abnormal findings",
                DiagnosisDate = new DateTime(2026, 9, 1),
            },
            new DiagnosisInfo
            {
                Sequence = 2,
                DiagnosisCodeType = "ICD-10",
                DiagnosisCode = "E11.9",
                DiagnosisDescription = "Type 2 diabetes mellitus without complications",
                DiagnosisDate = new DateTime(2026, 9, 1),
            },
        };

        // Service Details
        public List<ServiceDetail> ServiceDetails { get; set; } = new()
        {
            new ServiceDetail
            {
                LineNumber = 1,
                ProcedureCode = "99213",
                RequestedUnits = 1,
                RequestedDollars = 150m,
                RequestedFromDate = new DateTime(2026, 9, 1),
                RequestedToDate = new DateTime(2026, 9, 1),
            },
        };

        // Provider Notes
        public string ProviderNotes { get; set; } = "Patient requires a follow-up evaluation.";

        // Reviewer Notes
        public List<ReviewerNote> ReviewerNotes { get; set; } = new();

        // Outcome Of Review
        public List<OutcomeOfReview> Outcomes { get; set; } = new();

        // Attachments
        public List<AttachmentInfo> Attachments { get; set; } = new();
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
