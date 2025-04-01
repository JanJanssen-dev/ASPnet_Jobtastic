namespace ASPnet_Jobtastic.Models
{
    public class JobPostingModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string JobLocation { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int Salary { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public byte[]? CompanyImage { get; set; } 
        public string OwnerUsername { get; set; } = string.Empty;
    }
}
