namespace ResumeScreeningBusiness.Models
{
    public class ResumeEntitiesResponse
    {
        public List<string>? Skills { get; set; } = new List<string>();
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? CandidatePhoneNumber { get; set; }
        public int ResumeScore { get; set; }
        public List<string> Experiences { get; set; } = new List<string>();
        public List<string> ExperienceRanges { get; set; } = new List<string>();
    }
}
