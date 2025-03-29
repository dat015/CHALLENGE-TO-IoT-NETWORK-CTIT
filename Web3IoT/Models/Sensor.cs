using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web3IoT.Models
{
    public class Sensor
    {
        public Sensor()
        {
            Crops = new List<Crop>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên cảm biến")]
        [StringLength(100)]
        [Display(Name = "Tên cảm biến")]
        public required string Name { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập loại cảm biến")]
        // [StringLength(50)]
        // [Display(Name = "Loại cảm biến")]
        // public required string Type { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập vị trí lắp đặt")]
        // [StringLength(200)]
        // [Display(Name = "Vị trí lắp đặt")]
        // public required string Location { get; set; }

        // [Display(Name = "Ngày lắp đặt")]
        // [DataType(DataType.Date)]
        // public DateTime? InstallationDate { get; set; }

        // [Display(Name = "Trạng thái")]
        // [StringLength(20)]
        // public required string Status { get; set; }

        // [Display(Name = "Mô tả")]
        // public string? Description { get; set; }
        public int? SensorCode {get; set;}

        [Required]
        public int FarmId { get; set; }
        // Navigation properties
        public virtual Farm? Farm { get; set; }
        public virtual ICollection<Crop>? Crops { get; set; } = new List<Crop>();
    }
} 