using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasLessorCommunication
    {
        public string CrMasLessorCommunicationsLessorCode { get; set; } = null!;
        public string? CrMasLessorCommunicationsTgaContentType { get; set; }
        public string? CrMasLessorCommunicationsTgaAppId { get; set; }
        public string? CrMasLessorCommunicationsTgaAppKey { get; set; }
        public string? CrMasLessorCommunicationsTgaAuthorization { get; set; }
        public string? CrMasLessorCommunicationsTgaStatus { get; set; }
        public string? CrMasLessorCommunicationsShomoosAuthorization { get; set; }
        public string? CrMasLessorCommunicationsDefaultLanguage { get; set; }
        public string? CrMasLessorCommunicationsShomoosStatus { get; set; }
        public string? CrMasLessorCommunicationsWhatUpApi { get; set; }
        public string? CrMasLessorCommunicationsWhatUpDevice { get; set; }
        public string? CrMasLessorCommunicationsWhatUpStatus { get; set; }
        public string? CrMasLessorCommunicationsSmsName { get; set; }
        public string? CrMasLessorCommunicationsSmsApi { get; set; }
        public string? CrMasLessorCommunicationsSmsStatus { get; set; }

        public virtual CrMasLessorInformation CrMasLessorCommunicationsLessorCodeNavigation { get; set; } = null!;
    }
}
