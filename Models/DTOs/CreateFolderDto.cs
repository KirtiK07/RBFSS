using System.ComponentModel.DataAnnotations;

namespace RBFSS.Models.DTOs
{
    public class CreateFolderDto
    {
        [Required]
        [StringLength(255)]
        public string FolderName { get; set; } = string.Empty;

        public int? ParentFolderId { get; set; }
    }
}
