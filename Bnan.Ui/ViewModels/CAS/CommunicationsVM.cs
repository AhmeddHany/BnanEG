using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
    public class CommunicationsVM
    {
        public string? CrMasLessorCommunicationsLessorCode { get; set; }
        public string? CrMasLessorCommunicationsTgaContentType { get; set; }
        public string? CrMasLessorCommunicationsTgaAppId { get; set; }
        public string? CrMasLessorCommunicationsTgaAppKey { get; set; }
        public string? CrMasLessorCommunicationsTgaAuthorization { get; set; }
        public string? CrMasLessorCommunicationsTgaStatus { get; set; }
        public string? CrMasLessorCommunicationsShomoosContentType { get; set; }
        public string? CrMasLessorCommunicationsShomoosAppId { get; set; }
        public string? CrMasLessorCommunicationsShomoosAppKey { get; set; }
        public string? CrMasLessorCommunicationsShomoosAuthorization { get; set; }
        public string? CrMasLessorCommunicationsDefaultLanguage { get; set; }
        public string? CrMasLessorCommunicationsShomoosStatus { get; set; }
        public string? CrMasLessorCommunicationsSmsName { get; set; }
        public string? CrMasLessorCommunicationsSmsApi { get; set; }
        public string? CrMasLessorCommunicationsSmsStatus { get; set; }

        public virtual CrMasLessorInformation? CrMasLessorCommunicationsLessorCodeNavigation { get; set; }
    }
}
