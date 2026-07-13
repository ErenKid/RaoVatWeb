namespace RaoVatWeb.ViewModels
{
    public class AdminUserViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Roles { get; set; } = string.Empty;

        public int PostCount { get; set; }

        public bool IsLocked { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }
    }
}