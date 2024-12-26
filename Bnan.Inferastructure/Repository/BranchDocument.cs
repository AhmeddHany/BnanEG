using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class BranchDocument : IBranchDocument
    {
        private IUnitOfWork _unitOfWork;

        public BranchDocument(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddBranchDocument(string lessorCode, string branchCode)
        {
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(lessorCode);
            var sysProcedures = _unitOfWork.CrMasSysProcedure.FindAll(l => l.CrMasSysProceduresClassification == "10" && l.CrMasSysProceduresStatus == "A");
            if (lessor != null)
            {
                foreach (var item in sysProcedures)
                {
                    CrCasBranchDocument BranchDocument = new CrCasBranchDocument
                    {
                        CrCasBranchDocumentsLessor = lessor.CrMasLessorInformationCode,
                        CrCasBranchDocumentsBranch = branchCode,
                        CrCasBranchDocumentsProcedures = item.CrMasSysProceduresCode,
                        CrCasBranchDocumentsProceduresClassification = item.CrMasSysProceduresClassification,
                        CrCasBranchDocumentsActivation = true,
                        CrCasBranchDocumentsBranchStatus = lessor.CrMasLessorInformationStatus,
                        CrCasBranchDocumentsStatus = "N",

                    };

                    await _unitOfWork.CrCasBranchDocument.AddAsync(BranchDocument);
                }
            }

            return true;
        }



        public async Task<bool> AddBranchDocumentDefault(string lessorCode)
        {
            var sysProcedures = _unitOfWork.CrMasSysProcedure.FindAll(l => l.CrMasSysProceduresClassification == "10" && (l.CrMasSysProceduresStatus == "A" || l.CrMasSysProceduresStatus == "1"));
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(lessorCode);
            if (lessor != null)
            {
                foreach (var item in sysProcedures)
                {
                    CrCasBranchDocument BranchDocument = new CrCasBranchDocument
                    {
                        CrCasBranchDocumentsLessor = lessor.CrMasLessorInformationCode,
                        CrCasBranchDocumentsBranch = "100",
                        CrCasBranchDocumentsProcedures = item.CrMasSysProceduresCode,
                        CrCasBranchDocumentsProceduresClassification = item.CrMasSysProceduresClassification,
                        CrCasBranchDocumentsActivation = true,
                        CrCasBranchDocumentsBranchStatus = "A",
                        CrCasBranchDocumentsStatus = "N",

                    };

                    await _unitOfWork.CrCasBranchDocument.AddAsync(BranchDocument);
                }
            }

            return true;
        }

        public async Task<CrCasBranchDocument> GetBranchDocument(string DocumentsLessor, string DocumentsBranch, string DocumentsProcedures)
        {
            return await _unitOfWork.CrCasBranchDocument.FindAsync(l => l.CrCasBranchDocumentsLessor == DocumentsLessor && l.CrCasBranchDocumentsBranch == DocumentsBranch && l.CrCasBranchDocumentsProcedures == DocumentsProcedures);
        }

        public async Task<bool> UpdateBranchDocument(CrCasBranchDocument CrCasBranchDocument)
        {
            if (CrCasBranchDocument != null)
            {
                var document = await _unitOfWork.CrCasBranchDocument.FindAsync(l => l.CrCasBranchDocumentsBranch == CrCasBranchDocument.CrCasBranchDocumentsBranch
                                                                               && l.CrCasBranchDocumentsLessor == CrCasBranchDocument.CrCasBranchDocumentsLessor
                                                                               && l.CrCasBranchDocumentsProcedures == CrCasBranchDocument.CrCasBranchDocumentsProcedures);

                var AboutToExpire = _unitOfWork.CrCasLessorMechanism.FindAsync(l => l.CrCasLessorMechanismCode == document.CrCasBranchDocumentsLessor
                                                                                 && l.CrCasLessorMechanismProcedures == document.CrCasBranchDocumentsProcedures
                                                                                 && l.CrCasLessorMechanismProceduresClassification == document.CrCasBranchDocumentsProceduresClassification).Result.CrCasLessorMechanismDaysAlertAboutExpire;
                if (CrCasBranchDocument.CrCasBranchDocumentsStatus == Status.Renewed || CrCasBranchDocument.CrCasBranchDocumentsStatus == Status.Expire)
                {
                    document.CrCasBranchDocumentsStartDate = CrCasBranchDocument.CrCasBranchDocumentsStartDate;
                    document.CrCasBranchDocumentsEndDate = CrCasBranchDocument.CrCasBranchDocumentsEndDate;
                    document.CrCasBranchDocumentsDate = CrCasBranchDocument.CrCasBranchDocumentsDate;
                    document.CrCasBranchDocumentsNo = CrCasBranchDocument.CrCasBranchDocumentsNo;
                    document.CrCasBranchDocumentsImage = CrCasBranchDocument.CrCasBranchDocumentsImage;
                    document.CrCasBranchDocumentsReasons = CrCasBranchDocument.CrCasBranchDocumentsReasons;
                    document.CrCasBranchDocumentsDateAboutToFinish = CrCasBranchDocument.CrCasBranchDocumentsEndDate?.AddDays(-(double)AboutToExpire);
                    document.CrCasBranchDocumentsStatus = Status.Active;
                }
                else if (CrCasBranchDocument.CrCasBranchDocumentsStatus == Status.Deleted)
                {
                    document.CrCasBranchDocumentsStartDate = null;
                    document.CrCasBranchDocumentsEndDate = null;
                    document.CrCasBranchDocumentsDate = null;
                    document.CrCasBranchDocumentsNo = null;
                    document.CrCasBranchDocumentsImage = null;
                    document.CrCasBranchDocumentsReasons = null;
                    document.CrCasBranchDocumentsDateAboutToFinish = null;
                    document.CrCasBranchDocumentsStatus = Status.Renewed;
                }
                else
                {
                    document.CrCasBranchDocumentsImage = CrCasBranchDocument.CrCasBranchDocumentsImage;
                    document.CrCasBranchDocumentsReasons = CrCasBranchDocument.CrCasBranchDocumentsReasons;
                }


                _unitOfWork.CrCasBranchDocument.Update(document);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }
    }
}
