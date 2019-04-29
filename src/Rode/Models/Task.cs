namespace Rode.Models
{
    public class Task
    {
        public string Id { get; set; }

        public string OctopusEnvironmentName { get; set; }

        public string OctopusApplicationName { get; set; }

        public string ExecutablePath { get; set; }

        public string ExecutableArguments { get; set; }

        public string OverrideToAddresses { get; set; }

        public string OverrideFromAddress { get; set; }
    }
}