using RaoVatWeb.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class VipOrder
    {
        public int VipOrderId { get; set; }

        [Required]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public int VipPackageId { get; set; }

        public VipPackage? VipPackage { get; set; }

        public decimal Amount { get; set; }

        public VipOrderStatus Status { get; set; } = VipOrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        public DateTime? VipExpiredAt { get; set; }
    }
}