using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Rode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program will launch the Reminder process.");

            // Find the base application folder
            var baseFolderPath = ConfigurationManager.AppSettings["OctopusBaseApplicationDirectory"];

            if (!Directory.Exists(baseFolderPath))
            {
                Console.WriteLine("The directory was not found: " + baseFolderPath);
                return;
            }

            var baseFolder = new DirectoryInfo(baseFolderPath);
            var folders = baseFolder.GetDirectories().OrderByDescending(x => x.CreationTimeUtc).ToList();
            if (folders.Count == 0)
            {
                Console.WriteLine("No folders were found in the directory: " + baseFolder);
                return;
            }

            // Form the path:
            var exePath = ConfigurationManager.AppSettings["ExePath"];
            var path = string.Concat(baseFolder, folders.First().Name, "\\", exePath);
            if (!File.Exists(path))
            {
                Console.WriteLine("Could not find the path: " + path);
                return;
            }

            // Run the application:
            Console.WriteLine("Launching the application path: " + path);

            var psi = new ProcessStartInfo(path)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            Process.Start(psi);
        }
    }
}
