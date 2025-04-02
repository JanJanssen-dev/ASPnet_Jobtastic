namespace ASPnet_Jobtastic.Models
{
    public class JobSharingModel
    {
        public int Id { get; set; }

        // Fremdschlüssel zum JobPosting
        public int JobPostingId { get; set; }

        // Navigations-Property zum JobPosting
        public virtual JobPostingModel JobPosting { get; set; }

        // Benutzername des freigegebenen Benutzers
        public string SharedUsername { get; set; }

        // Datum der Freigabe
        public DateTime SharedDate { get; set; }

        // Benutzer, der die Freigabe erstellt hat
        public string SharedByUsername { get; set; }

        // Optionale Berechtigungen
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = false;
    }
}