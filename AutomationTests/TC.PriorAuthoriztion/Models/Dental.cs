using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TC.PriorAuthoriztion.Models
{
    public class DentalPA
    {
        public DentalInformation DentalInformation { get; set; }
        public DentalRecipientInformation DentalRecipientInformation { get; set; }
        public DentalContactInformation DentalContactInformation { get; set; }
        public DentalServiceInformation DentalServiceInformation { get; set; }
        public DentalServiceProviderInformation DentalServiceProviderInformation { get; set; }
        public DentalOrderingProviderInformation DentalOrderingProviderInformation { get; set; }
        public DentalDiagnosisInformation DentalDiagnosisInformation { get; set; }
        public DentalServiceDetails DentalServiceDetails { get; set; }
        public DentalProviderNotes DentalProviderNotes { get; set; }
        public DentalAttachments DentalAttachments { get; set; }
    }
    public class DentalInformation
    {
        public string RegID { get; set; }

        public string PAType { get; set; }

        public string DestinationPayerName { get; set; }

        public string Assignment { get; set; }

        public string ServiceType { get; set; }
    }
    public class DentalRecipientInformation
    {
        public string MedicaidBillingNumber { get; set; }
        public string DateOfBirth { get; set; }

        public string PatientTrackingNumber { get; set; }

    }
    public class DentalContactInformation
    {
        public string ContactFirstName { get; set; }

        public string ContactLastName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactExtension { get; set; }

    }
    public class DentalServiceInformation
    {
        public string PlaceOfService { get; set; }
        public string LevelOfService { get; set; }
        public string AccidentDate { get; set; }
        public string DelayReason { get; set; }
        public string AssociatePANo { get; set; }
        public string DateOfPatientEvent { get; set; }
        public string DateOfOnsetOfIllness { get; set; }
        public string DateOfLastMenstrualPeriod { get; set; }
        public string EstimatedDateOfBirth { get; set; }
    }
    public class DentalServiceProviderInformation
    {
        public string ServiceProviderNPI { get; set; }
    }
    public class DentalOrderingProviderInformation
    {
        public string OrderingProviderNPI { get; set; }
    }
    public class DentalDiagnosisInformation
    {
        public string DiagnosisCodeType { get; set; }
        public string DiagnosisCode { get; set; }
        public string DiagnosisCodeDescription { get; set; }
        public string DiagnosisDate { get; set; }
    }
    public class DentalServiceDetails
    {
        public string ProcedureCode { get; set; }
        public string RequestedUnits { get; set; }
        public string ProcedureCodeDescription { get; set; }
        public string RequestedDollars { get; set; }
        public string ToothNumber { get; set; }
        public string RequestedFDOS { get; set; }
        public string RequestedTDOS { get; set; }
        public string ServiceTrackingNo { get; set; }
        public string OralCavity { get; set; }
        public string ToothSurface { get; set; }
        public string ProviderServiceNote { get; set; }
        public string ProsthesisCrownOrInlay { get; set; }
    }
    public class DentalProviderNotes
    {
        public string ProviderNotes { get; set; }
    }
    public class DentalAttachments
    {
        public string FileName { get; set; }

        public string DocumentType { get; set; }

        public string AttachmentNotes { get; set; }
    }
}
