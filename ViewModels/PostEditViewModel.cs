using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.ViewModels
{
    public class PostEditViewModel
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin đăng")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả tin đăng")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khu vực")]
        public int AreaId { get; set; }
    }
}