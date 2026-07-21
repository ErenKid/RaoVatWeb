using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.ViewModels
{
    public class AccountSettingsViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsVip { get; set; }

        public DateTime? VipExpiredAt { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}