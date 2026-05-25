using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.ViewModels
{
    public class AdminAreaFormViewModel
    {
        public int? AreaId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khu vực")]
        [StringLength(100, ErrorMessage = "Tên khu vực không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        public int? ParentAreaId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}