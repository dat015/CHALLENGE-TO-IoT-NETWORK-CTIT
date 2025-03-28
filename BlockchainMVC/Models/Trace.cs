using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlockchainMVC.Models
{
    public class Trace
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã truy xuất là bắt buộc")]
        [Display(Name = "Mã truy xuất")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nông sản là bắt buộc")]
        [Display(Name = "Nông sản")]
        public int CropId { get; set; }

        [ForeignKey("CropId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Crop? Crop { get; set; }

        [Required(ErrorMessage = "Cây trồng là bắt buộc")]
        [Display(Name = "Cây trồng")]
        public int PlantId { get; set; }

        [ForeignKey("PlantId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Plant? Plant { get; set; }

        [Required(ErrorMessage = "Nguồn gốc là bắt buộc")]
        [Display(Name = "Nguồn gốc")]
        public int OriginId { get; set; }

        [ForeignKey("OriginId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Origin? Origin { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }
    }
} 