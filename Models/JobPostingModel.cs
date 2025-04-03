using System.ComponentModel.DataAnnotations.Schema;

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
        public string? ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? CompanyWebsite { get; set; } = string.Empty;
        public byte[]? CompanyImage { get; set; } 
        public string OwnerUsername { get; set; } = string.Empty;
        public DateTime? CreationDate { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string? ChangeUserName {  get; set; } = string.Empty;
        public string[]? AllowedUserNames { get; set; }

        [NotMapped]
        public bool IsOwner { get; set; }

        [NotMapped]
        public bool IsAdmin { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; }
    }
}
