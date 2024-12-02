using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasLessorMessage
    {
        public string CrMasLessorMessagesCode { get; set; } = null!;
        public string? CrMasLessorMessagesYear { get; set; }
        public string? CrMasLessorMessagesLessor { get; set; }
        public string? CrMasLessorMessagesBranch { get; set; }
        public string? CrMasLessorMessagesProcedures { get; set; }
        public string? CrMasLessorMessagesId { get; set; }
        public DateTime? CrMasLessorMessagesDateTime { get; set; }
        public string? CrMasLessorMessagesUserSender { get; set; }
        public string? CrMasLessorMessagesLessorReceiver { get; set; }
        public string? CrMasLessorMessagesRenterReceiver { get; set; }
        public string? CrMasLessorMessagesMobile { get; set; }
        public string? CrMasLessorMessagesFormat { get; set; }
        public string? CrMasLessorMessagesContent { get; set; }
        public string? CrMasLessorMessagesPath { get; set; }
        public string? CrMasLessorMessagesStatus { get; set; }

        public virtual CrCasBranchInformation? CrMasLessorMessages { get; set; }
        public virtual CrMasLessorInformation? CrMasLessorMessagesLessorReceiverNavigation { get; set; }
        public virtual CrMasSysProcedure? CrMasLessorMessagesProceduresNavigation { get; set; }
        public virtual CrMasRenterInformation? CrMasLessorMessagesRenterReceiverNavigation { get; set; }
        public virtual CrMasUserInformation? CrMasLessorMessagesUserSenderNavigation { get; set; }
    }
}
