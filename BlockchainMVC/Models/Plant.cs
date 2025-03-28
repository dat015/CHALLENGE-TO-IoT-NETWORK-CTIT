using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockchainMVC.Models
{
    public class Plant
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã cây là bắt buộc")]
        [Display(Name = "Mã cây")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên cây là bắt buộc")]
        [Display(Name = "Tên cây")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nông sản là bắt buộc")]
        [Display(Name = "Nông sản")]
        public int CropId { get; set; }

        [ForeignKey("CropId")]
        public Crop? Crop { get; set; }

        [Required(ErrorMessage = "Nguồn gốc là bắt buộc")]
        [Display(Name = "Nguồn gốc")]
        public int OriginId { get; set; }

        [ForeignKey("OriginId")]
        public Origin? Origin { get; set; }

        [Required(ErrorMessage = "Ngày trồng là bắt buộc")]
        [Display(Name = "Ngày trồng")]
        [DataType(DataType.Date)]
        public DateTime PlantingDate { get; set; }

        [Display(Name = "Ngày thu hoạch")]
        [DataType(DataType.Date)]
        public DateTime? HarvestDate { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 