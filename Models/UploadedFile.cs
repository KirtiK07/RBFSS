using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBFSS.Models
{
    public class UploadedFile
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        public string UploadedById { get; set; } = string.Empty;

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModified { get; set; }

        public bool IsDeleted { get; set; } = false;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int? FolderId { get; set; }

        [ForeignKey("UploadedById")]
        public virtual ApplicationUser UploadedBy { get; set; } = null!;

        [ForeignKey("FolderId")]
        public virtual Folder? Folder { get; set; }

        public virtual ICollection<FilePermission> FilePermissions { get; set; } = new List<FilePermission>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}