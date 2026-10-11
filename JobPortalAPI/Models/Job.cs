namespace JobPortalAPI.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public int EmployerId { get; set; }
        public User? Employer { get; set; }
    }
}