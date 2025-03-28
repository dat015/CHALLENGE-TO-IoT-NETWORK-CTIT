using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockchainMVC.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thiết bị là bắt buộc")]
        [Display(Name = "Tên thiết bị")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mã thiết bị là bắt buộc")]
        [Display(Name = "Mã thiết bị")]
        public string DeviceCode { get; set; }

        [Required(ErrorMessage = "Loại thiết bị là bắt buộc")]
        [Display(Name = "Loại thiết bị")]
        public string Type { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Vị trí")]
        public string Location { get; set; }

        [Display(Name = "Ngày cài đặt")]
        public DateTime? InstallationDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Mã hash khối")]
        public string BlockHash { get; set; }

        [Display(Name = "Cập nhật lúc")]
        public DateTime LastUpdate { get; set; }

        // Navigation property
        public virtual ICollection<Sensor> Sensors { get; set; }
    }
} 