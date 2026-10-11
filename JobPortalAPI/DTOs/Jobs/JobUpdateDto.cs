using System.ComponentModel.DataAnnotations;

namespace JobPortalAPI.DTOs.Jobs
{
    public class JobUpdateDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Range(0, 100000000)]
        public decimal Salary { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
    }
}