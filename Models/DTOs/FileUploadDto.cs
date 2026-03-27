using System.ComponentModel.DataAnnotations;

namespace RBFSS.Models.DTOs
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool ShareWithAllUsers { get; set; } = false;

        public List<string> SharedWithUserIds { get; set; } = new List<string>();

        public int? FolderId { get; set; }
    }
}