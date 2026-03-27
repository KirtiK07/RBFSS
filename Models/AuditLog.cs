using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBFSS.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // LOGIN, LOGOUT, UPLOAD, DOWNLOAD, DELETE, etc.

        [Required]
        [StringLength(500)]
        public string Resource { get; set; } = string.Empty; // What was accessed

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string IpAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string Details { get; set; } = string.Empty;

        public int? FileId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("FileId")]
        public virtual UploadedFile? File { get; set; }
    }
}