using System.ComponentModel.DataAnnotations;

namespace JobPortalAPI.DTOs.Applications
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        [RegularExpression("^(Reviewed|Shortlisted|Rejected|Accepted)$")]
        public string Status { get; set; } = string.Empty;
    }
}