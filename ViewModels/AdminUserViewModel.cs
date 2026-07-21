namespace RaoVatWeb.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsVip { get; set; }

        public DateTime? VipExpiredAt { get; set; }

        public bool IsLocked { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public string Roles { get; set; } = string.Empty;

        public int PostCount { get; set; }
    }
}