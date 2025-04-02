using System.Collections.Generic;

namespace ASPnet_Jobtastic.Models.ViewModels
{
    public class JobSharingViewModel
    {
        // Das JobPosting, für das Freigaben verwaltet werden
        public JobPostingModel JobPosting { get; set; }

        // Liste der bestehenden Freigaben
        public List<JobSharingModel> ExistingShares { get; set; } = new List<JobSharingModel>();

        // Model für eine neue Freigabe
        public JobSharingModel NewShare { get; set; } = new JobSharingModel();
    }
}