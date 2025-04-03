// Diese Klasse definiert die verschiedenen Operationen, die auf JobPostings ausgeführt werden können

namespace ASPnet_Jobtastic.Authorization
{
    public static class JobPostingOperations
    {
        public static readonly string View = "View";
        public static readonly string Edit = "Edit";
        public static readonly string Delete = "Delete";
        public static readonly string ManageSharing = "ManageSharing";
    }
}