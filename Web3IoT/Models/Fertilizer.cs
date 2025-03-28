using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web3IoT.Models
{
    public class Fertilizer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên phân bón là bắt buộc")]
        [Display(Name = "Tên phân bón")]
        public required string Name { get; set; }

        // [Required(ErrorMessage = "Mã phân bón là bắt buộc")]
        // [Display(Name = "Mã phân bón")]
        // public required string FertilizerCode { get; set; }

        [Required(ErrorMessage = "Loại phân bón là bắt buộc")]
        [Display(Name = "Loại phân bón")]
        public required string Type { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Display(Name = "Mô tả")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Display(Name = "Số lượng")]
        public required int Quantity { get; set; }
        [Required(ErrorMessage = "Mã cây trồng là bắt buộc")]
        [Display(Name = "Mã cây trồng")]
        public required int CropCode { get; set; }

        // [Required(ErrorMessage = "Nhà sản xuất là bắt buộc")]
        // [Display(Name = "Nhà sản xuất")]
        // public required string Manufacturer { get; set; }

        [Required(ErrorMessage = "Ngày bón phân là bắt buộc")]
        [Display(Name = "Ngày bón phân")]
        [DataType(DataType.Date)]
        public DateTime Timestamp { get; set; }

        // [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        // [Display(Name = "Ngày hết hạn")]
        // [DataType(DataType.Date)]
        // public DateTime ExpiryDate { get; set; }

        // [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        // [Display(Name = "Trạng thái")]
        // public required string Status { get; set; }

        // [Required(ErrorMessage = "Mã hash là bắt buộc")]
        // [Display(Name = "Mã hash")]
        // public required string BlockHash { get; set; }

        // [Required(ErrorMessage = "Thời gian cập nhật là bắt buộc")]
        // [Display(Name = "Cập nhật lúc")]
        // public DateTime LastUpdate { get; set; }
        
        public virtual Crop? Crops { get; set; }
    }
} 