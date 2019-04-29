using System.Collections.Generic;

namespace Rode.Models
{
    public class AppConfig
    {
        public string Version { get; set; }

        public string RodeInstanceName { get; set; }

        public string OctopusTentacleApplicationsBaseDirectory { get; set; }

        public string LogDirectory { get; set; }

        public List<Task> Tasks { get; set; }

        public EmailNotificationSettings EmailNotificationSettings { get; set; }

        public bool EnableLogging { get; set; }
    }

    public class EmailNotificationSettings
    {
        public bool Enabled { get; set; }

        public string DefaultToAddresses { get; set; }

        public string DefaultFromAddress { get; set; }
    }
}