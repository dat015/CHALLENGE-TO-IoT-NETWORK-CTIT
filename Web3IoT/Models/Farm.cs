using System.ComponentModel.DataAnnotations;

namespace Web3IoT.Models
{
    public class Farm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nông trại")]
        [StringLength(100)]
        [Display(Name = "Tên nông trại")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập diện tích")]
        [Display(Name = "Diện tích (ha)")]
        public decimal Area { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Cập nhật lúc")]
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<Crop> Crops { get; set; } = new List<Crop>();
        public virtual ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
        // public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
        // public virtual ICollection<Fertilizer> Fertilizers { get; set; } = new List<Fertilizer>();
        // public virtual ICollection<Pesticide> Pesticides { get; set; } = new List<Pesticide>();
    }
} 