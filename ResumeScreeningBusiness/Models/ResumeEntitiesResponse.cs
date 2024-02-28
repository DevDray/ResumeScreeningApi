namespace ResumeScreeningBusiness.Models
{
    public class ResumeEntitiesResponse
    {
        public List<string> DevelopmentSkills { get; set; } = new List<string>();
        public List<string> CloudSkills { get; set; } = new List<string>();
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? CandidatePhoneNumber { get; set; }
        public List<string> Experiences { get; set; } = new List<string>();
    }
}
