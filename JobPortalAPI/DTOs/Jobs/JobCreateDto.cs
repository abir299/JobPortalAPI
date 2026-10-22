
using System.ComponentModel.DataAnnotations;

namespace JobPortalAPI.DTOs.Jobs
{
    public class JobCreateDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Location { get; set; } = string.Empty;

        [Range(0, 100000000)]
        public decimal Salary { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^(Full-time|Part-time|Contract|Internship)$",
            ErrorMessage = "JobType must be Full-time, Part-time, Contract, or Internship.")]
        public string JobType { get; set; } = "Full-time";

        [Required]
        [RegularExpression(
            "^(Entry-level|Mid-level|Senior)$",
            ErrorMessage = "ExperienceLevel must be Entry-level, Mid-level, or Senior.")]
        public string ExperienceLevel { get; set; } = "Entry-level";

        public DateTime? ApplicationDeadline { get; set; }
    }
}
