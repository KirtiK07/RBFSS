using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBFSS.Models
{
    public class FilePermission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // READ, WRITE, DELETE

        [Required]
        [StringLength(100)]
        public string Resource { get; set; } = string.Empty; // File resource identifier

        [Required]
        public bool CanRead { get; set; } = false;

        [Required]
        public bool CanWrite { get; set; } = false;

        [Required]
        public bool CanDelete { get; set; } = false;

        [Required]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        [ForeignKey("FileId")]
        public virtual UploadedFile File { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}