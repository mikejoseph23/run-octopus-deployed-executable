using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;

namespace Rode.Code
{
    public abstract class TaskRunnerBase
    {
        public IList<ActivityLog> Logs { get; set; }

        public string LogFolderPath { get; set; }

        public string JobFolderPath { get; set; }

        // ---------------------------------------------------------------------------------
        // Sample Usage:
        // ---------------------------------------------------------------------------------
        //public void RunTask()
        //{
        //    CreateJobFolder();
        //
        //    try
        //    {
        //        // DO A BUNCH OF STUFF HERE. Use AppendToLog to add to the log file.
        //    }
        //    catch (Exception e)
        //    {
        //        AppendToLog(JsonConvert.SerializeObject(e), true);
        //        throw;
        //    }
        //    finally
        //    {
        //        WriteLogFile();
        //        var taskName = $"Automated Process ({_settings["Environment"]})";
        //        var fromAddress = _settings["NoReplyEmailAddress"];
        //        var recipients = _settings["NotifyRecipients"];
        //        EmailLog(fromAddress, recipients, taskName);
        //    }
        //}
        // ---------------------------------------------------------------------------------

        protected void CreateJobFolder()
        {
            var now = DateTime.UtcNow;
            var monthFolderPath = Path.Combine(LogFolderPath, $"{now.Year}-{To2Digits(now.Month)}");
            var dateTimeStamp = GetTimeStampString(now);
            var jobFolderPath = Path.Combine(monthFolderPath, dateTimeStamp);
            if (!Directory.Exists(LogFolderPath)) Directory.CreateDirectory(LogFolderPath);
            if (!Directory.Exists(monthFolderPath)) Directory.CreateDirectory(monthFolderPath);
            var jobFolder = Directory.CreateDirectory(jobFolderPath);
            JobFolderPath = jobFolder.FullName;
        }

        private string To2Digits(int i)
        {
            if (i.ToString().Length == 1) return "0" + i;
            return i.ToString();
        }

        private string GetTimeStampString(DateTime d, bool includeMilliseconds = false)
        {
            var retVal = $"{d.Year}-{To2Digits(d.Month)}-{To2Digits(d.Day)} {To2Digits(d.Hour)}.{To2Digits(d.Minute)}.{To2Digits(d.Second)}";
            if (includeMilliseconds) retVal = $"{retVal}.{d.Millisecond}";
            return retVal;
        }

        protected void AppendToLog(string message, bool isError = false)
        {
            var log = new ActivityLog { LogDate = DateTime.UtcNow, Message = message, IsError = isError };
            Logs.Add(log);
            Console.WriteLine(message);
        }

        protected void WriteLogFile(string filePath = "")
        {
            if (filePath == "") filePath = Path.Combine(JobFolderPath, "log.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(Logs, Formatting.Indented));
        }

        protected void EmailLog(string fromAddress, string recipientsSeparatedByCommas, string emailSubject)
        {
            if (Logs.Any(x => x.IsError)) emailSubject = $"Error Occurred - {emailSubject}";
            var msg = new MailMessage(fromAddress, recipientsSeparatedByCommas);
            msg.Subject = emailSubject;
            var attachmentStream = GenerateStreamFromString(GetHumanReadableLogContents());
            msg.Attachments.Add(new Attachment(attachmentStream, "log.txt"));
            msg.Body = "";
            msg.IsBodyHtml = false;
            var client = new SmtpClient();
            client.Send(msg);
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private string GetHumanReadableLogContents()
        {
            var sb = new StringBuilder();

            foreach (var log in Logs)
            {
                var code = log.IsError ? "ERROR" : "FYI";
                var line = $"{log.LogDate.ToShortDateString()} {log.LogDate.ToShortTimeString()} - {code} - {log.Message}";
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public class ActivityLog
        {
            public DateTime LogDate { get; set; }

            public string Message { get; set; }

            public bool IsError { get; set; }
        }
    }
}
