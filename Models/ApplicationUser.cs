using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RBFSS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UploadedFile> UploadedFiles { get; set; } = new List<UploadedFile>();
        public virtual ICollection<FilePermission> FilePermissions { get; set; } = new List<FilePermission>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Folder> CreatedFolders { get; set; } = new List<Folder>();
    }
}