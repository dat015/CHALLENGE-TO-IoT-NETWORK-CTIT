using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockchainMVC.Models
{
    public class Crop
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên cây trồng là bắt buộc")]
        [Display(Name = "Tên cây trồng")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cảm biến là bắt buộc")]
        [Display(Name = "Cảm biến")]
        public int SensorId { get; set; }

        [Display(Name = "Ngày trồng")]
        public DateTime? DatePlanted { get; set; }

        [Display(Name = "Ngày thu hoạch")]
        public DateTime? DateHarvested { get; set; }

        [Display(Name = "Loại cây")]
        public string Type { get; set; }

        [Display(Name = "Giống")]
        public string Variety { get; set; }

        [Display(Name = "Vị trí")]
        public string Location { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Hash Blockchain")]
        public string BlockHash { get; set; }

        [Display(Name = "Cập nhật lúc")]
        public DateTime LastUpdate { get; set; }

        [Display(Name = "ID Phân bón")]
        public string FertilizerId { get; set; }

        [Display(Name = "ID Thuốc trừ sâu")]
        public string PesticideId { get; set; }

        // Navigation property
        [ForeignKey("SensorId")]
        public Sensor Sensor { get; set; }
    }
} 