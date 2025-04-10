using ASPnet_Jobtastic.Models;

public class JobTableViewModel
{
    public List<JobPostingModel> Jobs { get; set; } = new List<JobPostingModel>();
    public string TableId { get; set; } = "jobsTable";
    public string EmptyMessage { get; set; } = "Keine Jobs vorhanden.";
    public bool ShowCreateButton { get; set; } = false;
}
