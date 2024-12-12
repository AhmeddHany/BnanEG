using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class QuestionsAnswerVM
    {
        [Required(ErrorMessage = "requiredFiled"), MaxLength(6, ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerNo { get; set; } = null!;
        [Required(ErrorMessage = "requiredFiled"), MaxLength(1, ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerSystem { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(3, ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerMainTask { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerArQuestions { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerArAnswer { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerEnQuestions { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasSysQuestionsAnswerEnAnswer { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysQuestionsAnswerArVideo { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysQuestionsAnswerEnVideo { get; set; }
        public string? CrMasSysQuestionsAnswerStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysQuestionsAnswerReasons { get; set; }

        public List<CrMasSysMainTask>? allMainTaskSystem = new List<CrMasSysMainTask>();

        public List<CrMasSysQuestionsAnswer>? crMasSysQuestionsAnswer = new List<CrMasSysQuestionsAnswer>();

        public List<CrMasSysSystem>? allsystemNames = new List<CrMasSysSystem>();

    }
}
