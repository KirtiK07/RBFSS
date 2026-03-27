using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBFSS.Models
{
    public class Folder
    {
        [Key]
        public int FolderId { get; set; }

        [Required]
        [StringLength(255)]
        public string FolderName { get; set; } = string.Empty;

        public int? ParentFolderId { get; set; }

        [Required]
        public string CreatedById { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("ParentFolderId")]
        public virtual Folder? ParentFolder { get; set; }

        public virtual ICollection<Folder> SubFolders { get; set; } = new List<Folder>();

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        public virtual ICollection<UploadedFile> Files { get; set; } = new List<UploadedFile>();
    }
}
