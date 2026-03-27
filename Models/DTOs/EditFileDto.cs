using System.ComponentModel.DataAnnotations;

namespace RBFSS.Models.DTOs
{
    public class EditFileDto
    {
        public int FileId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
