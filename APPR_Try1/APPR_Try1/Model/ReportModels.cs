using System.ComponentModel.DataAnnotations;

namespace APPR_Try1.Model
{
    public class ReportModels
    {
        [Required(ErrorMessage = "Report type is required")]
        public string ReportType { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string AdditionalFilters { get; set; }

        public string ReportData { get; set; }
    }
}
