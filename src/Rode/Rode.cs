using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Rode.Code;
using Rode.Models;

namespace Rode
{
    public class Rode: TaskRunnerBase
    {
        private AppConfig _config;
        private Task _task = null;
        private readonly StringBuilder _processOutput = new StringBuilder();

        public void RunTask(string taskId)
        {
            _config = GetConfig();
            LogFolderPath = _config.LogDirectory;
            Logs = new List<ActivityLog>();

            CreateJobFolder();

            try
            {
                RunRodeTask(taskId);
            }
            catch (Exception e)
            {
                AppendToLog(JsonConvert.SerializeObject(e), true);
                throw;
            }
            finally
            {
                if (_config.EnableLogging) WriteLogFile();
                if (_config.EmailNotificationSettings.Enabled)
                {
                    var emailSubject = $"{_task.Id} Process ({_task.OctopusEnvironmentName})";
                    var fromAddress = _config.EmailNotificationSettings.DefaultFromAddress;
                    var recipients = _config.EmailNotificationSettings.DefaultToAddresses;

                    if (_task != null)
                    {
                        if (!string.IsNullOrEmpty(_task.OverrideFromAddress)) fromAddress = _task.OverrideFromAddress;
                        if (!string.IsNullOrEmpty(_task.OverrideToAddresses)) fromAddress = _task.OverrideToAddresses;
                    }

                    EmailLog(fromAddress, recipients, emailSubject);
                }
            }
        }

        private void RunRodeTask(string taskId)
        {
            _task = _config.Tasks.SingleOrDefault(x => x.Id == taskId);
            if (_task == null)
            {
                AppendToLog("Could not find the task ID " + taskId, true);
                return;
            }

            AppendToLog($"Starting the '{_task.Id}' task.");

            // Find the base application folder
            var baseOctopusApplicationsFolderPath = _config.OctopusTentacleApplicationsBaseDirectory;
            if (!Directory.Exists(baseOctopusApplicationsFolderPath))
            {
                AppendToLog("The base Octopus 'Applications' directory was not found: " + baseOctopusApplicationsFolderPath, true);
                return;
            }

            var octopusApplicationFolderPath = Path.Combine(baseOctopusApplicationsFolderPath, _task.OctopusEnvironmentName,
                _task.OctopusApplicationName);
            var octopusApplicationFolder = new DirectoryInfo(octopusApplicationFolderPath);
            var folders = octopusApplicationFolder.GetDirectories().OrderByDescending(x => x.CreationTimeUtc).ToList();
            if (folders.Count == 0)
            {
                AppendToLog("No folders were found in the directory: " + octopusApplicationFolder, true);
                return;
            }

            // Form the path:
            var exePath = _task.ExecutablePath;
            var path = Path.Combine(octopusApplicationFolderPath, folders.First().Name, exePath);
            if (!File.Exists(path))
            {
                AppendToLog("Could not find the path: " + path, true);
                return;
            }

            // Run the application:
            AppendToLog("Launching the executable: " + path);
            LaunchExecutable(path, _task);
        }

        private void LaunchExecutable(string path, Task task)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var arguments = task.ExecutableArguments;
            if (!string.IsNullOrEmpty(arguments))
            {
                AppendToLog("Arguments: " + arguments);
                startInfo.Arguments = arguments;
            }

            var proc = new Process {StartInfo = startInfo};
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                _processOutput.AppendLine(line);
            }

            AppendToLog($"Process Output:\n{_processOutput}");
        }

        private AppConfig GetConfig()
        {
            var configPath = "./RodeConfig.json";
            var config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configPath));
            return config;
        }
    }
}
