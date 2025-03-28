using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockchainMVC.Models
{
    public class Sensor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên cảm biến là bắt buộc")]
        [Display(Name = "Tên cảm biến")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mã cảm biến là bắt buộc")]
        [Display(Name = "Mã cảm biến")]
        public string SensorCode { get; set; }

        [Display(Name = "Loại cảm biến")]
        public string Type { get; set; }

        [Display(Name = "Model")]
        public string Model { get; set; }

        [Display(Name = "Vị trí")]
        public string Location { get; set; }

        [Display(Name = "Ngày lắp đặt")]
        public DateTime InstallationDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Hash Blockchain")]
        public string BlockHash { get; set; }

        [Display(Name = "Cập nhật lúc")]
        public DateTime LastUpdate { get; set; }

        [Display(Name = "ID Cây trồng")]
        public string CropId { get; set; }

        // Navigation property
        public virtual ICollection<Crop> Crops { get; set; }
    }
} 