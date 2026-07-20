using RaoVatWeb.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class ReviewReport
    {
        public int ReviewReportId { get; set; }

        public int ReviewId { get; set; }

        public Review? Review { get; set; }

        [Required]
        public string ReporterId { get; set; } = string.Empty;

        public ApplicationUser? Reporter { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public ReviewReportStatus Status { get; set; } = ReviewReportStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ResolvedAt { get; set; }
    }
}