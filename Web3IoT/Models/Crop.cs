using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web3IoT.Models
{
    public class Crop
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã cây trồng là bắt buộc")]
        [Display(Name = "Mã cây trồng")]
        public int CropCode { get; set; }

        [Required(ErrorMessage = "Tên cây trồng là bắt buộc")]
        [Display(Name = "Tên cây trồng")]
        public string Name { get; set; }

        // [Required(ErrorMessage = "Loại cây là bắt buộc")]
        // [Display(Name = "Loại cây")]
        // public string Type { get; set; }

        // [Required(ErrorMessage = "Giống cây là bắt buộc")]
        // [Display(Name = "Giống cây")]
        // public string Variety { get; set; }

        [Required(ErrorMessage = "Vị trí trồng là bắt buộc")]
        [Display(Name = "Vị trí trồng")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Ngày trồng là bắt buộc")]
        [Display(Name = "Ngày trồng")]
        [DataType(DataType.Date)]
        public DateTime DatePlanted { get; set; }

        [Display(Name = "Ngày thu hoạch")]
        [DataType(DataType.Date)]
        public DateTime? DateHarvested { get; set; }

        // [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        // [Display(Name = "Trạng thái")]
        // public string Status { get; set; }

        // [Required(ErrorMessage = "Cập nhật lần cuối là bắt buộc")]
        // [Display(Name = "Cập nhật lần cuối")]
        // public DateTime LastUpdate { get; set; }

        // [Required(ErrorMessage = "Hash khối là bắt buộc")]
        // [Display(Name = "Hash khối")]
        // public string BlockHash { get; set; }

        // [Required(ErrorMessage = "ID nông trại là bắt buộc")]
        // [Display(Name = "ID nông trại")]
        // public int FarmId { get; set; }

        [Display(Name = "ID cảm biến")]
        public int? SensorId { get; set; }

        // [ForeignKey("FarmId")]
        // public Farm? Farm { get; set; }

        [ForeignKey("SensorId")]
        public Sensor? Sensor { get; set; }

        public List<Fertilizer>? Fertilizer { get; set; }

        public List<Pesticide>? Pesticide { get; set; }

        [Display(Name = "Mã QR")]
        public string? QRCode { get; set; }
    }
} 