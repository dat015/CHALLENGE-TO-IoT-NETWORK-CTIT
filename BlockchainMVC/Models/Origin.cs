using System.ComponentModel.DataAnnotations;

namespace BlockchainMVC.Models
{
    public class Origin
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nguồn gốc là bắt buộc")]
        [Display(Name = "Tên nguồn gốc")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 