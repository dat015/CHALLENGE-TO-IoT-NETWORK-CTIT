using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockchainMVC.Models
{
    public class Pesticide
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thuốc bảo vệ thực vật là bắt buộc")]
        [Display(Name = "Tên thuốc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mã thuốc là bắt buộc")]
        [Display(Name = "Mã thuốc")]
        public string PesticideCode { get; set; }

        [Required(ErrorMessage = "Loại thuốc là bắt buộc")]
        [Display(Name = "Loại thuốc")]
        public string Type { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Nhà sản xuất")]
        public string Manufacturer { get; set; }

        [Display(Name = "Ngày sản xuất")]
        public DateTime? ProductionDate { get; set; }

        [Display(Name = "Hạn sử dụng")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Mã hash khối")]
        public string BlockHash { get; set; }

        [Display(Name = "Cập nhật lúc")]
        public DateTime LastUpdate { get; set; }

        // Navigation property
        public virtual ICollection<Crop> Crops { get; set; }
    }
} 