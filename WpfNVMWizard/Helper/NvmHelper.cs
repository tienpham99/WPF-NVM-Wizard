using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpfNVMWizard.Helper
{
    public static class NvmHelper
    {
        public static string GetNvmPath()
        {
            string nvmHome = Environment.GetEnvironmentVariable("NVM_HOME");
            if (string.IsNullOrWhiteSpace(nvmHome))
            {
                // Nếu không có biến môi trường thì thử path mặc định
                nvmHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nvm");
            }

            string nvmExe = Path.Combine(nvmHome, "nvm.exe");
            return File.Exists(nvmExe) ? nvmExe : null;
        }

        public static bool IsNvmInstalled()
        {
            return !string.IsNullOrEmpty(GetNvmPath()) && File.Exists(GetNvmPath());
        }

        public static string RunNvmCommand(string args)
        {
            string nvmPath = GetNvmPath();
            if (string.IsNullOrEmpty(nvmPath)) return "nvm not found";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = nvmPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        public static List<(string version, bool isCurrent)> GetInstalledVersions()
        {
            var output = RunNvmCommand("list");
            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<(string version, bool isCurrent)>();

            foreach (var line in lines)
            {
                string l = line.Trim();
                if (string.IsNullOrWhiteSpace(l)) continue;

                bool isCurrent = l.StartsWith("*");

                var parts = l.Split(' ');

                string version = "";
                if (parts.Length > 1)
                    version = parts[1].Trim();
                else
                    version = l.Split(' ')[0].Replace("*", "").Trim();

                if (!string.IsNullOrWhiteSpace(version))
                    result.Add((version, isCurrent));
            }

            return result;
        }

        public static List<(string version, string date, string lts)> GetAvailableVersions()
        {
            using var client = new HttpClient();
            string json =  client.GetStringAsync("https://nodejs.org/dist/index.json").Result;

            var data = JArray.Parse(json);
            var versions = new List<(string version, string date, string lts)>();

            foreach (var item in data)
            {
                string version = item["version"]?.ToString();
                string date = item["date"]?.ToString();
                string lts = item["lts"]?.Type == JTokenType.String ? item["lts"]?.ToString() : null;

                versions.Add((version, date, lts));
            }

            return versions;
        }

        public static void InstallVersion(string version)
        {
            RunNvmCommand($"install {version}");
        }

        public static void UseVersion(string version)
        {
            RunNvmCommand($"use {version}");
        }

        public static void UninstallVersion(string version)
        {
            RunNvmCommand($"uninstall {version}");
        }

        public static string GetNvmVersion()
        {
            var output = RunNvmCommand("version");
            return output.Trim();
        }

        public static string GetNvmInstallPath()
        {
            return GetNvmPath()?.Replace("\\nvm.exe", "");
        }
    }
}
