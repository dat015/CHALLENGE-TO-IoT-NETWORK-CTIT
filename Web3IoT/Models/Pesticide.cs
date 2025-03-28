using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web3IoT.Models
{
    public class Pesticide
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thuốc bảo vệ thực vật")]
        [StringLength(100)]
        [Display(Name = "Tên thuốc")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập loại thuốc")]
        [StringLength(50)]
        [Display(Name = "Loại thuốc")]
        public required string Type { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập nhà sản xuất")]
        // [StringLength(100)]
        // [Display(Name = "Nhà sản xuất")]
        // public required string Manufacturer { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày phun thuốc")]
        [Display(Name = "Ngày phun thuốc")]
        [DataType(DataType.Date)]
        public DateTime Timestamp { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập hạn sử dụng")]
        // [Display(Name = "Hạn sử dụng")]
        // [DataType(DataType.Date)]
        // public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Display(Name = "Số lượng (lít)")]
        public decimal Quantity { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }


      
        [Required]
        [ForeignKey("Crop")]
        public int CropCode { get; set; }

        // Navigation properties
        public virtual Crop? Crops { get; set; }
    }
} 