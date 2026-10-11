namespace JobPortalAPI.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public Job? Job { get; set; }
        public int JobSeekerId { get; set; }
        public User? JobSeeker { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }
}