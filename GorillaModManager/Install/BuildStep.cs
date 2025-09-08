using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynamicData;
using GorillaModManager.Install;
using GorillaModManager.Models.Mods;
using GorillaModManager.Models.Persistence;
using GorillaModManager.Services;
using GorillaModManager.Views;

namespace GorillaModManager.Install;

public class BuildStep : IStep
{
    public async Task<bool> Run(string buildDirectory, BrowserMod mod)
    {
        string scriptPath = await GetBuildScript(buildDirectory, mod);
        if (string.IsNullOrEmpty(scriptPath))
        {
            await ItemInstaller.Notify("Failed", "Failed to get/find build cake.");
            return false;
        }

        // string path = Path.Combine(buildDirectory, "build.cake");
        // File.Move(scriptPath, path);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = buildDirectory,
            Arguments = $"dotnet cake --gamepath=\"{ManagerSettings.Default.GamePath}\"",
        };
        var process = Process.Start(processStartInfo);
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            await ItemInstaller.Notify("Failed", "Failed to build & install mod with cake");
            return false;
        }

        return true;
    }

    private async Task<string> GetBuildScript(string directory, BrowserMod mod)
    {
        string expectedBuildScript = Path.Combine(directory, "build.cake");
        if (File.Exists(expectedBuildScript)) return expectedBuildScript;

        using var httpClient = new HttpClient();

        string url = $"{BrowserService.REMOTE_URL}/build_scripts/{mod.ModName.Trim()}.cake";
        url = Regex.Replace(url, @"\s+", "");

        var result = await httpClient.GetAsync(url) ?? null;
        if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine("Url invalid " + url);
            return null;
        }

        string content = await result.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(expectedBuildScript, content);
        return expectedBuildScript;
    }
}
