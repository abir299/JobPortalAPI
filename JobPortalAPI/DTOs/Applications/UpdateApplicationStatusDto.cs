
using System.ComponentModel.DataAnnotations;

namespace JobPortalAPI.DTOs.Applications
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        [RegularExpression(
            "^(Pending|Reviewed|Shortlisted|Rejected|Accepted)$",
            ErrorMessage = "Status must be Pending, Reviewed, Shortlisted, Rejected, or Accepted.")]
        public string Status { get; set; } = string.Empty;
    }
}
