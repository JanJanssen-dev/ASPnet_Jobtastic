// Diese Klasse definiert die Anforderung für die Autorisierung

using Microsoft.AspNetCore.Authorization;

namespace ASPnet_Jobtastic.Authorization
{
    public class JobPostingAuthorizationRequirement : IAuthorizationRequirement
    {
        public string Operation { get; }

        public JobPostingAuthorizationRequirement(string operation)
        {
            Operation = operation;
        }
    }
}