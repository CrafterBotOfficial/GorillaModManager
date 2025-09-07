using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GorillaModManager.Install;
using GorillaModManager.Models.Mods;
using GorillaModManager.Services;

namespace GorillaModManager;

public class DownloadStep : IStep
{
    public async Task<bool> Run(string buildDirectory, BrowserMod mod)
    {
        if (Directory.Exists(buildDirectory))
        {
            return true;
        }

        var gitCloneProcess = Process.Start("git", $"clone {mod.GitUrl} \"{buildDirectory}\"");
        await gitCloneProcess.WaitForExitAsync();
        if (gitCloneProcess.ExitCode != 0)
        {
            await ItemInstaller.Notify("Error", "Failed to download from " + mod.GitUrl);
            return false;
        }

        var gitCheckoutProcessInfo = new ProcessStartInfo
        {
            WorkingDirectory = buildDirectory,
            FileName = "git",
            Arguments = $"checkout {mod.CommitHash}"
        };
        var gitCheckoutProcess = Process.Start(gitCheckoutProcessInfo);
        await gitCheckoutProcess.WaitForExitAsync();
        if (gitCheckoutProcess.ExitCode != 0)
        {
            await ItemInstaller.Notify("Error", "Failed to checkout specific git commit.");
            return false;
        }

        return true;
    }
}
